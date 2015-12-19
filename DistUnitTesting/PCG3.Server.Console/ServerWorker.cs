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


    //bytearray 
    [XcoConcurrent]
    public void Process(AssemblyRequest assembly) {
      var response = new AssemblyResponse();
      try {
        Assembly.Load(assembly.Bytes);
        response.Worked = true;
        Console.WriteLine("Assembly got load on Server.");
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

    public int Cores { get; set; }
    public List<Task> Tasks { get; set; }

    //public ServerTestWorker(int Cores) {
    //  //this.Cores = Cores;
    //  //for (int i = 1; i <= Cores; i++) {
    //  //  Tasks.Add(new Task())
    //  //}
    //}

    [XcoConcurrent]
    public void Process(TestRequest testRequest) {
      TestResponse response = new TestResponse();

      TestRunner tr = new TestRunner();

      response.Result =
         tr.RunTest(testRequest.Test);


      // //Tasks (nicht Threads)
      // //waitany
      //// for 
      // Task t = Task.Run( () => {
      //   // Just loop.
      //   int ctr = 0;
      //   for (ctr = 0; ctr <= 1000000; ctr++)
      //                             {}
      //   Console.WriteLine("Finished {0} loop iterations",
      //                                               ctr);
      // } );
      // t.Wait();


      //response.result = Testresult;
      testRequest.ResponsePort.Post(response);
    }
  }
}
