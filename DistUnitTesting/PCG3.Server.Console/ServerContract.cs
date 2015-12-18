using PCG3.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XcoAppSpaces.Core;
using System.Reflection;

namespace PCG3.Server.Console {
  class ServerAssemblyWorker : AssemblyWorker {


    //bytearray 
    [XcoConcurrent]
    public void Process(AssemblyRequest assembly) {
      var response = new AssemblyResponse();
      try {
        Assembly.Load(assembly.Bytes);
        response.Worked = true;
      }
      catch (Exception e) {
        response.Worked = false;
        response.ErrorMsg = e.ToString();
      }
      finally {
        assembly.ResponsePort.Post(response);
      }
    }
  }


  public class ServerTestWorker : TestWorker {

    [XcoConcurrent]
    public void Process(TestRequest test) {
      var response = new TestResponse();

      

      //response.result = Testresult;
      test.ResponsePort.Post(response);
    }
  }
}
