using PCG3.Middleware;
using System;
using XcoAppSpaces.Core;

namespace PCG3.Server {

  public class Program {

    private const int DEFAULT_CORES = 4;
    private const int DEFAULT_PORT  = 9000;

    public static void Main(string[] args) {

      int cores = DEFAULT_CORES;
      int port = DEFAULT_PORT;

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
      
      using (var space = new XcoAppSpace(string.Format("tcp.port={0}", port))) {
        Console.WriteLine("Number of cores: {0}", cores);

        // create workers to be run within the application space
        TestWorker worker = space.RunWorker<TestWorker, ServerTestWorker>();
        Console.WriteLine(string.Format("TestWorker on {0}:     running...", port));
        space.RunWorker<AssemblyWorker, ServerAssemblyWorker>();
        Console.WriteLine(string.Format("AssemblyWorker on {0}: running...", port));
        

        Console.ReadLine();
      }
    }
  }
}
