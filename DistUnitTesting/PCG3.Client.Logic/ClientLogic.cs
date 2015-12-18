using PCG3.Middleware;
using System;
using System.IO;
using XcoAppSpaces.Core;

namespace PCG3.Client.Logic {

  public class ClientLogic {

    private const string APP_SPACE_CONFIG_STRING = "tcp.port=0";

    public XcoAppSpace Space { get; private set; }

    public ClientLogic() {
      this.Space = new XcoAppSpace(APP_SPACE_CONFIG_STRING);
    }


    public void DeployAssemblyToServer(string assemblyPath, string serverAddress) {

      
      AssemblyWorker worker = Space.ConnectWorker<AssemblyWorker>(serverAddress);
      AssemblyRequest request = new AssemblyRequest();
      request.Bytes = File.ReadAllBytes(assemblyPath);
      request.ResponsePort = Space.Receive<AssemblyResponse>(resp => {
        if (!resp.Worked) {
          // print error message if assembly deployment failed
          Console.WriteLine(resp.ErrorMsg);
        }
      });

      worker.Post(request);
    }
  }
}
