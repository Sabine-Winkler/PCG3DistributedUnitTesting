using PCG3.Client.ViewModel.ViewModel;
using System.Windows;

namespace PCG3.Client.GUI {
  
  public partial class MainWindow : Window {

    public MainWindow() {
      InitializeComponent();
      DataContext = new MainVM();
    }
  }
}
