using PCG3.Middleware;
using PCG3.TestFramework;
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

    public void SendTestsToServer(TestResults tests, string[] serverAddresses) {


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

    private List<TestResult> AssemblyToList(string assemblyPath) {
      List<TestResult> list = new List<TestResult>();
      Assembly assembly = Assembly.LoadFrom(assemblyPath);

      TestResults results = new TestResults();

      foreach (Type type in assembly.GetTypes()) {

        object testClass = type.Assembly.CreateInstance(type.FullName);

        foreach (MethodInfo method in type.GetMethods()) {
          TestResult test = new TestResult();
          test.MethodInfo = method;
          list.Add(test);
        }
      }


      return list;
    }
  }
}
