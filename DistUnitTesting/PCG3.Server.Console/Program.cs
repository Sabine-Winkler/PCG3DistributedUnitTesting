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
          port = int.Parse(args[0]);
        }
        catch (Exception) {
          port = 9000;
        }
      }
      #endregion




      Console.WriteLine("Xco Application Space - Distributed Workers Server");
      using (var space = new XcoAppSpace($"tcp.port={port}")) {
        //run worker in server space
        space.RunWorker<AssemblyWorker, ServerAssemblyWorker>();
        //space.RunWorker<TestWorker, ServerTestWorker>();
        Console.WriteLine("running...");

        Console.ReadLine();
      }


    }



  }
}
