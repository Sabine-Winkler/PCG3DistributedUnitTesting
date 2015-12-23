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

  /// <summary>
  /// Logic of the clients.
  /// Extracts tests from an assembly, deploys the assembly to the servers,
  /// and distributes the tests to the servers using a three step approach
  /// (allocate, run, free) and a publish-subscribe mechanism.
  /// </summary>
  public class ClientLogic {

    private const string APP_SPACE_CONFIG_STRING = "tcp.port=0";
 
    #region message templates
    private const string TEMPLATE_ASSEMBLY_DEPLOY_FAILED
      = "[Client/Logic] Failed to deploy the assembly '{0}' on the server {1}. Error: {2}";
    private const string TEMPLATE_ASSEMBLY_DEPLOY_SUCCEED
      = "[Client/Logic] Successfully deployed the assembly '{0}' on the server {1}.";
    private const string TEMPLATE_TEST_RESULT
      = "[Client/Logic] [Result]\n{0}\n";
    private const string TEMPLATE_ERROR
      = "[Client/Logic] An error occurred: {0}";
    private const string TEMPLATE_CLIENT_WAIT
      = "[Client/Logic] Sleep 1 sec.";
    private const string TEMPLATE_ALLOC
      = "[Client/Logic] Server: {0}, Allocated: {1}";
    private const string TEMPLATE_FREE_CORE
      = "[Client/Logic] Free core.";
    private const string TEMPLATE_TESTS_NOT_COMPLETED
      = "[Client/Logic] {0} tests not completed yet.";
    #endregion

    private const string TEMPLATE_WORKER_LOCAL_NAME
      = "{0}/{1}";


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

          // A local name is required per AssemblyWorker, because it is not allowed
          // to create multiple instances of AssemblyWorker within one app space.
          string localName = string.Format(TEMPLATE_WORKER_LOCAL_NAME, addr, i);
          
          AssemblyWorker worker = space.ConnectWorker<AssemblyWorker>(addr, localName);

          AssemblyRequest request    = new AssemblyRequest();
          request.AssemblyPath       = assemblyPath;
          request.AssemblyByteStream = File.ReadAllBytes(assemblyPath);
          request.ResponsePort       = space.Receive<AssemblyResponse>(resp => {
            
            string message = "";
            if (!resp.Worked) {
              message = string.Format(TEMPLATE_ASSEMBLY_DEPLOY_FAILED,
                                      assemblyPath, addr, resp.ErrorMsg);

            } else {
              message = string.Format(TEMPLATE_ASSEMBLY_DEPLOY_SUCCEED,
                                      assemblyPath, addr);
            }
            Logger.Log(message);
            countdown.Signal();
          });

          worker.Post(request);
        }

        // This method is left and the app space is closed
        // after the assembly has been deployed on each server.
        countdown.Wait();
      };

    }


    public delegate void OnTestCompleted(Test test);

    /// <summary>
    /// Distributes a list of tests to an array of servers.
    /// If a test was executed by a server and the result is returned,
    /// the client's view model is notified by the given OnTestCompleted callback.
    /// </summary>
    /// <param name="tests">tests to distribute</param>
    /// <param name="serverAddresses">array of server addresses in the form host:port</param>
    /// <param name="callback">callback called when a server returned a result of a test</param>
    public void DistributeTestsToServers(List<Test> availableTests, string[] serverAddresses,
                                         OnTestCompleted callback) {

      int serverCount = serverAddresses.Length;
      int testCount   = availableTests.Count;
      TestWorker[] testWorkers = new TestWorker[serverCount];

      // step 1 - set the status of each test to "In Progress"
      foreach (Test test in availableTests) {
        test.Status = TestStatus.IN_PROGRESS;
        callback(test);
      }
      
      using (XcoAppSpace space = new XcoAppSpace(APP_SPACE_CONFIG_STRING)) {
        

        // step 2 - connect to each server and create a worker per server
        for (int i = 0; i < serverCount; ++i) {
          string addr = serverAddresses[i];
          string localName = string.Format(TEMPLATE_WORKER_LOCAL_NAME, addr, i);
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

          Logger.Log(TEMPLATE_CLIENT_WAIT);
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

              string message = string.Format(TEMPLATE_ALLOC, allocReq.ServerAddress, cores);
              Logger.Log(message);
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
                  message = string.Format(TEMPLATE_TEST_RESULT, runResp.Result);
                  Logger.Log(message);
                  lock (lockObj) {
                    callback(runResp.Result);
                  }
                  
                  FreeCoreRequest freeReq = new FreeCoreRequest();
                  #region free response
                  freeReq.ResponsePort = space.Receive<FreeCoreResponse>(freeResp => {
                    Logger.Log(TEMPLATE_FREE_CORE);
                    testsCd.Signal();
                    message = string.Format(TEMPLATE_TESTS_NOT_COMPLETED, testsCd.CurrentCount);
                    Logger.Log(message);
                  });
                  #endregion free response

                  testWorkers[runReq.ServerId].Post(freeReq);
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

        testsCd.Wait(); // wait until all tests have been finished
      }
    }


    /// <summary>
    /// Extracts and returns a list of methods of a given assembly, which
    /// have an attribute [Test].
    /// </summary>
    /// <param name="assemblyPath">assembly to extract the tests from</param>
    /// <returns>list of methods having an attribute [Test]</returns>
    public List<Test> GetTestMethodsOfAssembly(string assemblyPath) {

      List<Test> tests = new List<Test>();

      // load assembly
      Assembly assembly = Assembly.LoadFrom(assemblyPath);

      foreach (Type type in assembly.GetTypes()) {

        foreach (MethodInfo method in type.GetMethods()) {

          object[] attributes = method.GetCustomAttributes(false);
          TestAttribute testAttribute
            = Utilities.FindAttribute<TestAttribute>(attributes);

          if (testAttribute != null) {
            Test test       = new Test();
            test.MethodName = method.Name;
            test.Status     = TestStatus.NONE;
            test.Type       = type;
            tests.Add(test);
          }
        }
      }
      return tests;
    }
  }
}
