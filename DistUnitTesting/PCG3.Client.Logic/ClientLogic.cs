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

    private XcoAppSpace space;

    public ClientLogic() {
      this.space = new XcoAppSpace(APP_SPACE_CONFIG_STRING);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assemblyPath"></param>
    /// <param name="serverAddress"></param>
    public void DeployAssemblyToServer(string assemblyPath, string serverAddress) {

      AssemblyWorker serverWorker = space.ConnectWorker<AssemblyWorker>(serverAddress);
      AssemblyRequest deployAssemblyRequest = new AssemblyRequest();
      deployAssemblyRequest.Bytes = File.ReadAllBytes(assemblyPath);
      deployAssemblyRequest.ResponsePort = space.Receive<AssemblyResponse>(resp => {
        if (!resp.Worked) {
          // print error message if assembly deployment failed
          Console.WriteLine(resp.ErrorMsg);
        }
      });

      serverWorker.Post(deployAssemblyRequest);
    }

    public void SendTestsToServer(List<TestResult> tests, string[] serverAddresses) {


      //Test first test
      TestResult test = tests[0];
      string serverAddress = serverAddresses[0];
      TestWorker worker = space.ConnectWorker<TestWorker>(serverAddress);
      TestRequest request = new TestRequest();
      request.Test = test;

      //Parallel here




      request.ResponsePort = space.Receive<TestResponse>(resp => {
        Console.WriteLine(resp.Result);
      });


      worker.Post(request);
    }

    public List<TestResult> GetTestMethodsOfAssembly(string assemblyPath) {

      List<TestResult> tests = new List<TestResult>();
      Assembly assembly = Assembly.LoadFrom(assemblyPath);

      foreach (Type type in assembly.GetTypes()) {

        foreach (MethodInfo method in type.GetMethods()) {

          object[] attributes = method.GetCustomAttributes(false);
          TestAttribute testAttribute
            = Utilities.FindAttribute<TestAttribute>(attributes);

          if (testAttribute != null) {
            TestResult test = new TestResult();
            test.MethodInfo = method;
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
