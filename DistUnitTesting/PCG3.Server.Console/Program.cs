using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using XcoAppSpaces.Core;
using PCG3.Middleware;

namespace PCG3.Server {

  public class Program {

    public static void Main(string[] args) {

      int cores = 4, port = 9000;
      #region parametercheck
      if (args.Length > 0) { }
      try {
        cores = int.Parse(args[0]);
      }
      catch (Exception) {
        cores = 4;
      }
      if (args.Length > 1) {
        try {
          port = int.Parse(args[1]);
        }
        catch (Exception) {
          port = 9000;
        }
      }
      #endregion


      Console.WriteLine("Xco Application Space - Distributed Unittesting Server");
      using (var space = new XcoAppSpace(string.Format("tcp.port={0}", port))) {
        //run worker in server space
        space.RunWorker<AssemblyWorker, ServerAssemblyWorker>();
        Console.WriteLine(string.Format("AssemblyWorker on {0}: running...", port));
        space.RunWorker<TestWorker, ServerTestWorker>();
        Console.WriteLine(string.Format("TestWorker on {0}: running...", port));

        Console.ReadLine();
      }


    }



  }
}
