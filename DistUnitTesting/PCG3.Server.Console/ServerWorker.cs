using PCG3.Middleware;
using PCG3.TestFramework;
using PCG3.Util;
using System;
using System.Reflection;
using System.Threading.Tasks;
using XcoAppSpaces.Core;

namespace PCG3.Server {


  /// <summary>
  /// Server worker for deploying and loading the assembly.
  /// </summary>
  public class ServerAssemblyWorker : AssemblyWorker {

    private const string ASSEMBLY_LOAD_TEMPLATE
      = "{0}: Assembly got load on server.";

    [XcoExclusive]
    public void Process(AssemblyRequest req) {

      AssemblyResponse resp = new AssemblyResponse();

      try {
        Assembly.Load(req.Bytes);
        resp.Worked = true;
        string message = string.Format(ASSEMBLY_LOAD_TEMPLATE, DateUtil.GetCurrentDateTime());
        Console.WriteLine(message);
      } catch (Exception e) {
        resp.Worked = false;
        resp.ErrorMsg = e.ToString();
      } finally {
        req.ResponsePort.Post(resp);
      }
    }
  }


  /// <summary>
  /// Server worker for running tests on a server.
  /// </summary>
  public class ServerTestWorker : TestWorker {

    private const string TEST_PROCESS_TEMPLATE
      = "{0}: [Processing] {1} {2}()";
    private const string TEST_RESULT_TEMPLATE
      = "{0}: [Result]{1}{2}{3}";

    // XcoPublisher automatically manages incoming subscriptions
    private readonly XcoPublisher<AllocCoresRequest> allocCoresPublisher
      = new XcoPublisher<AllocCoresRequest>();
    private readonly XcoPublisher<FreeCoreRequest> freeCorePublisher
      = new XcoPublisher<FreeCoreRequest>();

    [XcoExclusive]
    public void Process(AllocCoresRequest req) {
      Console.WriteLine("Process: Allocate cores for " + req.TestCount + " tests maximally.");
      allocCoresPublisher.Publish(req);
    }

    [XcoExclusive]
    public void Process(FreeCoreRequest req) {
      Console.WriteLine("Process: Free core.");
      freeCorePublisher.Publish(req);
    }


    [XcoExclusive]
    public void Process(RunTestsRequest req) {

      Console.WriteLine("Process: Run tests.");

      foreach (Test test in req.Tests) {
        
        Task.Run(() => {
          RunTestsResponse resp = new RunTestsResponse();
          Console.WriteLine("Process: Running test ...");
          resp.Result = new TestRunner().RunTest(test);
          Console.WriteLine("Process: Finished test.");
          req.ResponsePort.Post(resp);
        });
      }
    }
  }
}