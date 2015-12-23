using PCG3.Client.ViewModel.ViewModel;
using System;
using System.Windows;

namespace PCG3.Client.GUI {

  public partial class MainWindow : Window {

    public MainWindow() {

      Console.WriteLine("======================================");
      Console.WriteLine("  CLIENT for Distributed Unittesting  ");
      Console.WriteLine("     using Xco Application Space      ");
      Console.WriteLine("======================================");

      InitializeComponent();
      
      #region check program arguments
      string[] args = Environment.GetCommandLineArgs();

      string assembly        = "";
      string serverAddresses = "";

      if (args.Length > 1) {
        assembly = args[1];
      }

      if (args.Length > 2) {
        for (int i = 2; i < args.Length; ++i) {
          serverAddresses += args[i] + Environment.NewLine;
        }
      }
      #endregion

      DataContext = new MainVM(assembly, serverAddresses, this.Dispatcher);
    }
  }

}