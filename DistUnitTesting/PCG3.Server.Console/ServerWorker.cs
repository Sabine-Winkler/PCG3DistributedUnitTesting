using PCG3.Middleware;
using PCG3.TestFramework;
using System;
using System.Reflection;
using XcoAppSpaces.Core;

namespace PCG3.Server {

  public class ServerAssemblyWorker : AssemblyWorker {

    [XcoConcurrent]
    public void Process(AssemblyRequest assemblyRequest) {

      var response = new AssemblyResponse();
    
      try {
        Assembly.Load(assemblyRequest.Bytes);
        response.Worked = true;
        Console.WriteLine("Assembly got load on Server.");
      } catch (Exception e) {
        response.Worked = false;
        response.ErrorMsg = e.ToString();
      } finally {
        assemblyRequest.ResponsePort.Post(response);
      }
    }
  }


  public class ServerTestWorker : TestWorker {

    [XcoConcurrent]
    public void Process(TestRequest testRequest) {
      
      Console.WriteLine("####> in Process: " + testRequest.Test.MethodName);
      TestResponse response = new TestResponse();

      TestRunner tr = new TestRunner();

      response.Result =
         tr.RunTest(testRequest.Test);

      Console.WriteLine("Result: " + response.Result);

      testRequest.ResponsePort.Post(response);
    }
  }
}
