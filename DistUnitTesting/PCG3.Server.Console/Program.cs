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

      int cores = 4; //standard
      if (args.Length > 0) {
        cores = Int32.Parse(args[0]);
      }
   

      Console.WriteLine("Xco Application Space - Distributed Workers Server");
      using (var space = new XcoAppSpace("tcp.port=9000")) {
        //run worker in server space
        space.RunWorker<AssemblyWorker, ServerAssemblyWorker>();
        //space.RunWorker<TestWorker, ServerTestWorker>();
        Console.WriteLine("running...");

        Console.ReadLine();
      }


    }
  }
}
