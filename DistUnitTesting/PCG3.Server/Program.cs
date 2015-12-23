using Microsoft.Ccr.Core;
using PCG3.Middleware;
using PCG3.Util;
using System;
using XcoAppSpaces.Core;

namespace PCG3.Server {

  /// <summary>
  /// Main program for the server.
  /// </summary>
  public class Program {

    #region message templates
    private const string TEMPLATE_RUNNING
      = "[S/Progr] [Me|{0}] Running {1} on {2}.";
    private const string TEMPLATE_NO_CORES
      = "[S/Progr] [Me|{0}] Using {1} cores.";
    private const string TEMPLATE_FREE_CORE
      = "[S/Progr] [Me|{0}] [C|{1}] Free core.";
    private const string TEMPLATE_ALLOC
      = "[S/Progr] [Me|{0}] [C|{1}] Requested: {2}, Allocated: {3}, Free: {4}";
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

      Console.WriteLine("======================================");
      Console.WriteLine("  SERVER for Distributed Unittesting  ");
      Console.WriteLine("     using Xco Application Space      ");
      Console.WriteLine("======================================");

      string message;

      using (var space = new XcoAppSpace(string.Format("tcp.port={0}", port))) {
        
        string serverAddress = space.Address;

        message = string.Format(TEMPLATE_NO_CORES, serverAddress, cores);
        Logger.Log(message);

        // create workers to be run within the application space
        TestWorker worker = space.RunWorker<TestWorker, ServerTestWorker>();
        space.RunWorker<AssemblyWorker, ServerAssemblyWorker>();
        
        Logger.Log(string.Format(TEMPLATE_RUNNING, serverAddress, "TestWorker", port));
        Logger.Log(string.Format(TEMPLATE_RUNNING, serverAddress, "AssemblyWorker", port));


        Object logLockObj   = new Object();
        Object coresLockObj = new Object();

        // allocate cores subscription
        Port<AllocCoresRequest> allocSubscription
          = space.Receive<AllocCoresRequest>(req => {
              
              AllocCoresResponse resp = new AllocCoresResponse();

              lock (coresLockObj) {
                if (req.TestCount >= cores) {
                  resp.AllocCores = cores;
                } else {
                  resp.AllocCores = req.TestCount;
                }
                cores = cores - resp.AllocCores;

                message = string.Format(TEMPLATE_ALLOC,
                                        serverAddress, req.ClientAddress,
                                        req.TestCount, resp.AllocCores, cores);
              }
              lock(logLockObj) { Logger.Log(message); }

              req.ResponsePort.Post(resp);
            });

        worker.Post(new Subscribe<AllocCoresRequest>(allocSubscription));


        // free core subscription
        Port<FreeCoreRequest> freeSubscription
          = space.Receive<FreeCoreRequest>(req => {

              message = string.Format(TEMPLATE_FREE_CORE,
                                      serverAddress, req.ClientAddress);

              lock (logLockObj) { Logger.Log(message); }

              lock (coresLockObj) {
                cores++;
              }

              req.ResponsePort.Post(new FreeCoreResponse());
          });

        worker.Post(new Subscribe<FreeCoreRequest>(freeSubscription));


        // wait until the user terminates the server pressing a key
        Console.ReadLine();
      }
    }
  }
}
