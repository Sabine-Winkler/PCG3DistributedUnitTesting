using PCG3.Middleware;
using PCG3.TestFramework;
using PCG3.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
      = "{0}: The server {1} has {2} available cores.";
    private const string TEST_RESULT_TEMPLATE
      = "{0}: [Result]{1}{2}{3}";
    private const string ERROR_TEMPLATE
      = "{0}: [ERROR] An error occurred: {1}";
    #endregion


    public ClientLogic() {
      // currently nothing to do
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="assemblyPath"></param>
    /// <param name="serverAddresses"></param>
    public void DeployAssemblyToServers(string assemblyPath, string[] serverAddresses) {

      using (var space = new XcoAppSpace(APP_SPACE_CONFIG_STRING)) {

        int i = 0;

        foreach (string serverAddress in serverAddresses) {

          AssemblyWorker serverWorker
            = space.ConnectWorker<AssemblyWorker>(serverAddress, serverAddress + "/" + i);

          i++;

          AssemblyRequest deployAssemblyRequest = new AssemblyRequest();

          deployAssemblyRequest.Bytes = File.ReadAllBytes(assemblyPath);

          deployAssemblyRequest.ResponsePort = space.Receive<AssemblyResponse>(resp => {
            string message = "";
            if (!resp.Worked) {
              message = string.Format(ASSEMBLY_DEPLOY_FAILED_TEMPLATE,
                                      DateUtil.GetCurrentDateTime(),
                                      assemblyPath, serverAddress, resp.ErrorMsg);

            } else {
              message = string.Format(ASSEMBLY_DEPLOY_SUCCEED_TEMPLATE,
                                      DateUtil.GetCurrentDateTime(),
                                      assemblyPath, serverAddress);
            }
            Console.WriteLine(message);
          });

          serverWorker.Post(deployAssemblyRequest);
        }
      };
    }


    public delegate void TestCompletedCallback(Test test);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="availableTests"></param>
    /// <param name="serverAddresses"></param>
    /// <param name="callback"></param>
    public void SendTestsToServers(List<Test> availableTests, string[] serverAddresses,
                                   TestCompletedCallback callback) {

      // set the status of each test to Waiting...
      foreach (Test test in availableTests) {
        test.Status = TestStatus.WAITING;
        callback(test);
      }

      //int index = 0;
      
      //using (var space = new XcoAppSpace(APP_SPACE_CONFIG_STRING)) {
      //  while (index < tests.Count) {

      //    foreach (string serverAddress in serverAddresses) {

      //      try {
      //        TestWorker worker = space.ConnectWorker<TestWorker>(serverAddress, string.Format("{0}{1}", serverAddress, index.ToString()) );

      //        if ( worker.AvailableCores() > 0) {

      //          List<Test> testsToServer = new List<Test>();
      //          for(int i=0; i < worker.AvailableCores();i++) {
      //            testsToServer.Add(tests[index]);
      //            index++;
      //          }

      //          Console.WriteLine("###> worker has core(s) available");

      //          TestRequestTest requestTest = new TestRequestTest();
      //          requestTest.Tests = testsToServer;
      //          requestTest.ResponsePort = space.Receive<TestResponseResult>(response => {

      //            foreach (Test result in response.Results) {
      //              Console.WriteLine("###> Testresult: " + result);
      //              insertInList(tests, result);
      //            }

      //          });


      //          worker.Post(requestTest);
                
      //        }
      //        else {
      //          Console.WriteLine("###> worker has no core available");
      //        }
      //      }
      //      catch (Exception) {
      //        Console.WriteLine("###> There is already an instance of {0}{1}", serverAddress, index.ToString());

      //      }
      //    }
      //  };
      //}


      ////request.ResponsePort = space.Receive<TestResponse>(resp => {
      ////  Console.WriteLine("###> " + resp.Result);
      ////});


      ////worker.Post(request);

      ///*foreach (Test t in tests) {

      //  TestRequest request = new TestRequest();
      //  request.Test = t;
      //  request.ResponsePort = sp.Receive<TestResponse>(resp => {
      //    Console.WriteLine("###> " + resp.Result);
      //    Console.WriteLine(resp.Result.MethodInfo.Name);
      //  });

      //  Console.WriteLine("***> " + t.MethodInfo);
      //  worker.Post(request);
      //}
      //}*/
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
