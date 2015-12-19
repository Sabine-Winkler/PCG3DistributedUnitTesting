using PCG3.Middleware;
using System;
using System.IO;
using XcoAppSpaces.Core;

namespace PCG3.Client.Logic {

  public class ClientLogic {

    private const string APP_SPACE_CONFIG_STRING = "tcp.port=0";

    private XcoAppSpace space;

    public ClientLogic() {
      this.space = new XcoAppSpace(APP_SPACE_CONFIG_STRING);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assemblyPath"></param>
    /// <param name="serverAddress"></param>
    public void DeployAssemblyToServer(string assemblyPath, string serverAddress) {

      AssemblyWorker serverWorker = space.ConnectWorker<AssemblyWorker>(serverAddress);
      AssemblyRequest deployAssemblyRequest = new AssemblyRequest();
      deployAssemblyRequest.Bytes = File.ReadAllBytes(assemblyPath);
      deployAssemblyRequest.ResponsePort = space.Receive<AssemblyResponse>(resp => {
        if (!resp.Worked) {
          // print error message if assembly deployment failed
          Console.WriteLine(resp.ErrorMsg);
        }
      });

      serverWorker.Post(deployAssemblyRequest);
    }
  }
}
