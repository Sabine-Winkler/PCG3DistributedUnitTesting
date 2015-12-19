using PCG3.Client.ViewModel.ViewModel;
using System;
using System.Windows;

namespace PCG3.Client.GUI {

  public partial class MainWindow : Window {

    public MainWindow() {
      InitializeComponent();
      string[] args = Environment.GetCommandLineArgs();

      string assembly = "";
      string serverAddresses = "";

      if (args.Length > 1) {
        assembly = args[1];
      }

      if (args.Length > 2) {
        for (int i = 2; i < args.Length; ++i) {
          serverAddresses += args[i] + Environment.NewLine;
        }
      }

      DataContext = new MainVM(assembly, serverAddresses);
    }
  }

}