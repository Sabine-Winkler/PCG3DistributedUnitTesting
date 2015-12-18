using PCG3.Client.ViewModel.ViewModel;
using PCG3.Middleware;
using System.IO;
using System.Windows;
using XcoAppSpaces.Core;
using System;

namespace PCG3.Client.GUI {

  public partial class MainWindow : Window {

    public MainWindow() {
      InitializeComponent();
      DataContext = new MainVM();
    }


    private const string WORKER_ADDRESS = "localhost:9000";
    private const string APP_SPACE_CONFIG_STRING = "tcp.port=0";
    private const string PATH = "F:\\MSc\\3_PCG\\UET2\\PCG3.TestUnitTests.dll";


    private void SelectAssemblyButton_Click(object sender, RoutedEventArgs e) {

      XcoAppSpace space = new XcoAppSpace(APP_SPACE_CONFIG_STRING);
      var worker = space.ConnectWorker<AssemblyWorker>(WORKER_ADDRESS);


      var request = new AssemblyRequest();
      request.Bytes = File.ReadAllBytes(PATH);
      request.ResponsePort = space.Receive<AssemblyResponse>(resp => {
        if (!resp.Worked) {
          // Response.ErrorMsg ausgeben
          Console.WriteLine(resp.ErrorMsg);
        }
      });

      worker.Post(request);

    }
  }

}
  

