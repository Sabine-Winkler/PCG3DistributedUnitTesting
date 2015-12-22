using Microsoft.Ccr.Core;
using PCG3.Middleware;
using PCG3.Util;
using System;
using XcoAppSpaces.Core;

namespace PCG3.Server {

  public class Program {

    private const string RUNNING_TEMPLATE
      = "{0}: Running {1} on {2}.";
    private const string NO_CORES_TEMPLATE
      = "{0}: Using {1} cores.";

    private const int DEFAULT_CORES = 4;
    private const int DEFAULT_PORT  = 9000;

    public static void Main(string[] args) {

      int cores = DEFAULT_CORES;
      int port  = DEFAULT_PORT;

      #region parameter check
      if (args.Length > 0) {
        try {
          cores = int.Parse(args[0]);
        } catch (Exception) {
          cores = DEFAULT_CORES;
        }
      }
      if (args.Length > 1) {
        try {
          port = int.Parse(args[1]);
        } catch (Exception) {
          port = DEFAULT_PORT;
        }
      }
      #endregion


      Console.WriteLine("Xco Application Space - Distributed Unittesting Server");
      
      string message;

      using (var space = new XcoAppSpace(string.Format("tcp.port={0}", port))) {
        
        message = string.Format(NO_CORES_TEMPLATE, DateUtil.GetCurrentDateTime(), cores);
        Console.WriteLine(message);

        // create workers to be run within the application space
        TestWorker worker = space.RunWorker<TestWorker, ServerTestWorker>();
        space.RunWorker<AssemblyWorker, ServerAssemblyWorker>();
        
        message = string.Format(RUNNING_TEMPLATE, DateUtil.GetCurrentDateTime(), "TestWorker", port);
        Console.WriteLine(message);
        message = string.Format(RUNNING_TEMPLATE, DateUtil.GetCurrentDateTime(), "AssemblyWorker", port);
        Console.WriteLine(message);
        
        Port<AllocCoresRequest> allocSubscription
          = space.Receive<AllocCoresRequest>(req => {
              
              AllocCoresResponse resp = new AllocCoresResponse();

              if (req.TestCount >= cores) {
                resp.AllocCores = cores;
              } else {
                resp.AllocCores = req.TestCount;
              }
              cores = cores - resp.AllocCores;

              Console.WriteLine(
                "Client: Requested: " + req.TestCount
                + ", Allocated: " + resp.AllocCores
                + ", Left: " + cores);

              req.ResponsePort.Post(resp);
            });

        worker.Post(new Subscribe<AllocCoresRequest>(allocSubscription));

        Port<FreeCoreRequest> freeSubscription
          = space.Receive<FreeCoreRequest>(req => {

              Console.WriteLine("Client: Free core.");

              cores++;

              req.ResponsePort.Post(new FreeCoreResponse());
          });

        worker.Post(new Subscribe<FreeCoreRequest>(freeSubscription));
                
        Console.ReadLine();
      }
    }
  }
}
