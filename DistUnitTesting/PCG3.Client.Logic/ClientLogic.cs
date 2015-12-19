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

    ~ClientLogic() {
      this.space.Dispose();
    }
    

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assemblyPath"></param>
    /// <param name="serverAddresses"></param>
    public void DeployAssemblyToServer(string assemblyPath, string[] serverAddresses) {
      
        foreach (string serverAddress in serverAddresses) {
            AssemblyWorker serverWorker = space.ConnectWorker<AssemblyWorker>(serverAddress);
            AssemblyRequest deployAssemblyRequest = new AssemblyRequest();
            deployAssemblyRequest.Bytes = File.ReadAllBytes(assemblyPath);
            deployAssemblyRequest.ResponsePort = space.Receive<AssemblyResponse>(resp => {
              if (!resp.Worked) {
                Console.WriteLine("###> " + resp.ErrorMsg);
              }
              else {
                Console.WriteLine("###> Deployment of Assembly worked");
              }
            });

            serverWorker.Post(deployAssemblyRequest);
          }
      
    }


  public void SendTestsToServer(List<Test> tests, string[] serverAddresses) {

     
        //Test first test
        Test test = tests[0];
        string serverAddress = serverAddresses[0];
        TestWorker worker = space.ConnectWorker<TestWorker>(serverAddress);
        TestRequest request = new TestRequest();
        request.Test = test;

        request.ResponsePort = space.Receive<TestResponse>(resp => {
          Console.WriteLine("###> " + resp.Result);
        });


        worker.Post(request);

        /*foreach (Test t in tests) {

          TestRequest request = new TestRequest();
          request.Test = t;
          request.ResponsePort = sp.Receive<TestResponse>(resp => {
            Console.WriteLine("###> " + resp.Result);
            Console.WriteLine(resp.Result.MethodInfo.Name);
          });

          Console.WriteLine("***> " + t.MethodInfo);
          worker.Post(request);
        }
      }*/


       
      
    }


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
