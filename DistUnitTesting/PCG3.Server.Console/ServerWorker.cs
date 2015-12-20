using PCG3.Middleware;
using PCG3.TestFramework;
using PCG3.Util;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using XcoAppSpaces.Core;

namespace PCG3.Server {

  public class ServerAssemblyWorker : AssemblyWorker {

    private const string ASSEMBLY_LOAD_TEMPLATE
      = "{0}: Assembly got load on server.";

    [XcoExclusive]
    public void Process(AssemblyRequest assemblyRequest) {

      AssemblyResponse response = new AssemblyResponse();

      try {
        Assembly.Load(assemblyRequest.Bytes);
        response.Worked = true;
        string message = string.Format(ASSEMBLY_LOAD_TEMPLATE, DateUtil.GetCurrentDateTime());
        Console.WriteLine(message);
      } catch (Exception e) {
        response.Worked = false;
        response.ErrorMsg = e.ToString();
      } finally {
        assemblyRequest.ResponsePort.Post(response);
      }
    }
  }

  public class ServerTestWorker : TestWorker {

    private const string TEST_PROCESS_TEMPLATE
      = "{0}: [Processing] {1} {2}()";
    private const string TEST_RESULT_TEMPLATE
      = "{0}: [Result]{1}{2}{3}";

    [XcoExclusive]
    public void Process(TestRequestTest testRequest) {

      TestResponseResult response = new TestResponseResult();

      List<Test> results = new List<Test>();

      Parallel.ForEach(testRequest.Tests, test => {

        string message = string.Format(TEST_PROCESS_TEMPLATE, DateUtil.GetCurrentDateTime(),
                                       test.Type.FullName, test.MethodName);
        Console.WriteLine(message);

        // coresInUse++;
        TestRunner tr = new TestRunner();
        Test result = tr.RunTest(test);
        results.Add(result);
        // coresInUse--;

        message = string.Format(TEST_RESULT_TEMPLATE, DateUtil.GetCurrentDateTime(),
                                Environment.NewLine, result, Environment.NewLine);
        Console.WriteLine(message);
      });

      response.Results = results;
      testRequest.ResponsePort.Post(response);
    }
  }
}
  

