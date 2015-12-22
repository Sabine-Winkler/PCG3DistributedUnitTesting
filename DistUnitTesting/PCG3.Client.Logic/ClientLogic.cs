using PCG3.Middleware;
using PCG3.TestFramework;
using PCG3.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using XcoAppSpaces.Core;

namespace PCG3.Client.Logic {

  public class ClientLogic {

    private const string APP_SPACE_CONFIG_STRING = "tcp.port=0";
 
    #region message templates
    private const string ASSEMBLY_DEPLOY_FAILED_TEMPLATE
      = "{0}: Failed to deploy the assembly '{1}' on the server {2}. Error: {3}";
    private const string ASSEMBLY_DEPLOY_SUCCEED_TEMPLATE
      = "{0}: Successfully deployed the assembly '{1}' on the server {2}.";
    private const string AVAILABLE_CORES_TEMPLATE
      = "{0}: The server {1} ({2}) has {3} available cores.";
    private const string TEST_RESULT_TEMPLATE
      = "{0}: [Result]{1}{2}{3}";
    private const string ERROR_TEMPLATE
      = "{0}: [ERROR] An error occurred: {1}";
    #endregion


    public ClientLogic() {
      // currently nothing to do
    }


    /// <summary>
    /// Serializes the assembly with the given assemblyPath and
    /// sends the serialized assembly to a set of servers.
    /// </summary>
    /// <param name="assemblyPath">relative or absolute path to an assembly (.dll, .exe)</param>
    /// <param name="serverAddresses">array of server address in the form host:port</param>
    public void DeployAssemblyToServers(string assemblyPath, string[] serverAddresses) {

      using (var space = new XcoAppSpace(APP_SPACE_CONFIG_STRING)) {

        CountdownEvent countdown = new CountdownEvent(serverAddresses.Length);

        for (int i = 0; i < serverAddresses.Length; ++i) {

          string addr = serverAddresses[i];
          string localName = addr + "/" + i;
          AssemblyWorker worker = space.ConnectWorker<AssemblyWorker>(addr, localName);

          AssemblyRequest request = new AssemblyRequest();
          request.Bytes = File.ReadAllBytes(assemblyPath);
          request.ResponsePort = space.Receive<AssemblyResponse>(resp => {
            
            string message = "";
            if (!resp.Worked) {
              message = string.Format(ASSEMBLY_DEPLOY_FAILED_TEMPLATE,
                                      DateUtil.GetCurrentDateTime(),
                                      assemblyPath, addr, resp.ErrorMsg);

            } else {
              message = string.Format(ASSEMBLY_DEPLOY_SUCCEED_TEMPLATE,
                                      DateUtil.GetCurrentDateTime(),
                                      assemblyPath, addr);
            }
            Console.WriteLine(message);
            countdown.Signal();
          });

          worker.Post(request);
        }

        countdown.Wait();
      };

    }


    public delegate void OnTestCompleted(Test test);

    /// <summary>
    /// Distributes a list of tests to an array of servers.
    /// If a test was executed by a server and the result is returned,
    /// the client's view model is notified by the given callback.
    /// </summary>
    /// <param name="tests">tests to distribute</param>
    /// <param name="serverAddresses">array of server addresses in the form host:port</param>
    public void SendTestsToServers(List<Test> availableTests, string[] serverAddresses,
                                   OnTestCompleted callback) {

      int serverCount = serverAddresses.Length;
      int testCount   = availableTests.Count;
      TestWorker[] testWorkers = new TestWorker[serverCount];
      
      // step 1 - set the status of each test to Waiting...
      foreach (Test test in availableTests) {
        test.Status = TestStatus.WAITING;
        callback(test);
      }
      
      using (XcoAppSpace space = new XcoAppSpace(APP_SPACE_CONFIG_STRING)) {
        

        // step 2 - connect to each server and create a worker per server
        for (int i = 0; i < serverCount; ++i) {
          string addr = serverAddresses[i];
          string localName = addr + "/" + i;
          testWorkers[i] = space.ConnectWorker<TestWorker>(addr, localName);
          testWorkers[i].ServerId = i;
          testWorkers[i].ServerAddress = addr;
          testWorkers[i].LocalName = localName;
        }

        CountdownEvent testsCd = new CountdownEvent(testCount);
        Object lockObj = new Object();

        // step 3 - distribute tests to the servers
        int freeTestsCount = testCount;
        int testNo = 0;

        while (freeTestsCount > 0) {

          Console.WriteLine("Client: Sleep 1 sec.");
          Thread.Sleep(1000);

          foreach (TestWorker worker in testWorkers) {

            CountdownEvent allocCd = new CountdownEvent(1);
               
            AllocCoresRequest allocReq = new AllocCoresRequest();
            allocReq.ServerId      = worker.ServerId;
            allocReq.ServerAddress = worker.ServerAddress;
            allocReq.TestCount     = freeTestsCount;

            #region alloc response
            allocReq.ResponsePort  = space.Receive<AllocCoresResponse>(allocResp => {
              
              int cores = allocResp.AllocCores;

              Console.WriteLine("Client: Server: " + allocReq.ServerAddress + ", Allocated: " + cores);
              freeTestsCount -= cores;

              allocCd.Signal();

              if (cores > 0) {
                List<Test> assignedTests = new List<Test>();
                for (int j = 0; j < cores; ++j) {
                  assignedTests.Add(availableTests[testNo]);
                  testNo++;
                }

                RunTestsRequest runReq = new RunTestsRequest();
                runReq.ServerId = allocReq.ServerId;
                runReq.ServerAddress = allocReq.ServerAddress;
                runReq.Tests = assignedTests;

                #region run response
                runReq.ResponsePort = space.Receive<RunTestsResponse>(runResp => {
                  Console.WriteLine("Client: " + runResp.Result);
                  lock (lockObj) {
                    callback(runResp.Result);
                  }
                  
                  CountdownEvent freeCd = new CountdownEvent(1);
                  FreeCoreRequest freeReq = new FreeCoreRequest();

                  #region free response
                  freeReq.ResponsePort = space.Receive<FreeCoreResponse>(freeResp => {
                    Console.WriteLine("Client: Free core.");
                    testsCd.Signal();
                    Console.WriteLine("Client: " + testsCd.CurrentCount + " tests are not completed yet.");
                  });
                  #endregion free response

                  testWorkers[runReq.ServerId].Post(freeReq);
                  Console.WriteLine("Client: Try to free core.");

                });
                #endregion run response

                testWorkers[allocReq.ServerId].Post(runReq);
              }

            });
            #endregion alloc response

            testWorkers[allocReq.ServerId].Post(allocReq);
            allocCd.Wait();

          } // foreach worker

        } // while enough tests

        testsCd.Wait();
      }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="assemblyPath"></param>
    /// <returns></returns>
    public List<Test> GetTestMethodsOfAssembly(string assemblyPath) {

      List<Test> tests = new List<Test>();
      Assembly assembly = Assembly.LoadFrom(assemblyPath);

      foreach (Type type in assembly.GetTypes()) {

        foreach (MethodInfo method in type.GetMethods()) {

          object[] attributes = method.GetCustomAttributes(false);
          TestAttribute testAttribute
            = Utilities.FindAttribute<TestAttribute>(attributes);

          if (testAttribute != null) {
            Test test = new Test();
            test.MethodName = method.Name;
            test.Status = TestStatus.NONE;
            test.Type = type;
            tests.Add(test);
          }
        }
      }
      return tests;
    }
  }
}
