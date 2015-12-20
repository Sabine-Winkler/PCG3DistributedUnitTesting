using PCG3.Middleware;
using PCG3.TestFramework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using XcoAppSpaces.Core;

namespace PCG3.Server {

  public class ServerAssemblyWorker : AssemblyWorker {

    [XcoExclusive]
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

    [XcoExclusive]
    public void Process(TestRequestTest testRequest) {
      var response = new TestResponseResult();
      List<Test> results = new List<Test>();
      Parallel.ForEach(testRequest.Tests, test => {
        Console.WriteLine("####> in Process: " + test.MethodName);


        coresInUse++;
        TestRunner tr = new TestRunner();
        Test result =  tr.RunTest(test);
        results.Add(result);
        coresInUse--;


        //Console.WriteLine("######> {0} of {1} available cores in use", coresInUse, Cores);
        Console.WriteLine("###> Result: " + result);


      });
      response.Results = results;
      testRequest.ResponsePort.Post(response);

    }

    //[XcoConcurrent]
    //private Test runTest(Test test) {
    //  TestRunner tr = new TestRunner();
    //  return tr.RunTest(test);
    //}
  

  }
}
  

