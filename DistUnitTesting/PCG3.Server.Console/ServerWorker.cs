using PCG3.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XcoAppSpaces.Core;
using System.Reflection;
using PCG3.TestFramework;

namespace PCG3.Server {
  class ServerAssemblyWorker : AssemblyWorker {

    [XcoConcurrent]
    public void Process(AssemblyRequest assemblyRequest) {
      var response = new AssemblyResponse();
      try {
        Assembly.Load(assemblyRequest.Bytes);
        response.Worked = true;
        Console.WriteLine("Assembly got load on Server.");
      }
      catch (Exception e) {
        response.Worked = false;
        response.ErrorMsg = e.ToString();
      }
      finally {
        assemblyRequest.ResponsePort.Post(response);
      }


      
    }
  }


  public class ServerTestWorker : TestWorker {

    [XcoConcurrent]
    public void Process(TestRequest testRequest) {
      
      Console.WriteLine("####> in Process: " + testRequest);
      TestResponse response = new TestResponse();

      TestRunner tr = new TestRunner();

      response.Result =
         tr.RunTest(testRequest.Test);

      testRequest.ResponsePort.Post(response);
    }
  }
}
