using Microsoft.Ccr.Core;
using PCG3.Middleware;
using PCG3.Util;
using System;
using XcoAppSpaces.Core;

namespace PCG3.Server {

  public class Program {

    #region message templates
    private const string TEMPLATE_RUNNING
      = "[Server/Progr] Running {0} on {1}.";
    private const string TEMPLATE_NO_CORES
      = "[Server/Progr] Using {0} cores.";
    private const string TEMPLATE_FREE_CORE
      = "[Server/Progr] Free core.";
    private const string TEMPLATE_ALLOC
      = "[Server/Progr] Client - Requested: {0}, Allocated: {1}, Free: {2}";
    #endregion

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

      Console.WriteLine("==========================================================");
      Console.WriteLine("  Xco Application Space - Distributed Unittesting Server  ");
      Console.WriteLine("==========================================================");

      string message;

      using (var space = new XcoAppSpace(string.Format("tcp.port={0}", port))) {
        
        message = string.Format(TEMPLATE_NO_CORES, cores);
        Logger.Log(message);

        // create workers to be run within the application space
        TestWorker worker = space.RunWorker<TestWorker, ServerTestWorker>();
        space.RunWorker<AssemblyWorker, ServerAssemblyWorker>();
        
        Logger.Log(string.Format(TEMPLATE_RUNNING, "TestWorker", port));
        Logger.Log(string.Format(TEMPLATE_RUNNING, "AssemblyWorker", port));


        // allocate cores subscription
        Port<AllocCoresRequest> allocSubscription
          = space.Receive<AllocCoresRequest>(req => {
              
              AllocCoresResponse resp = new AllocCoresResponse();

              if (req.TestCount >= cores) {
                resp.AllocCores = cores;
              } else {
                resp.AllocCores = req.TestCount;
              }
              cores = cores - resp.AllocCores;

              message = string.Format(TEMPLATE_ALLOC, req.TestCount, resp.AllocCores, cores);
              Logger.Log(message);

              req.ResponsePort.Post(resp);
            });

        worker.Post(new Subscribe<AllocCoresRequest>(allocSubscription));


        // free core subscription
        Port<FreeCoreRequest> freeSubscription
          = space.Receive<FreeCoreRequest>(req => {

              Logger.Log(TEMPLATE_FREE_CORE);

              cores++;

              req.ResponsePort.Post(new FreeCoreResponse());
          });

        worker.Post(new Subscribe<FreeCoreRequest>(freeSubscription));

        // wait until the user terminates the server pressing a key
        Console.ReadLine();
      }
    }
  }
}
