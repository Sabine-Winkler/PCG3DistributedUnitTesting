using Microsoft.Win32;
using PCG3.Client.Logic;
using System;
using System.Windows.Input;

namespace PCG3.Client.ViewModel.ViewModel {

  public class MainVM : ViewModelBase<MainVM> {

    private string selectedAssemblyPath;
    private string serverAddresses;
    private ClientLogic logic;

    public MainVM() {
      logic = new ClientLogic();
    }

    public string SelectedAssemblyPath {
      
      get { return selectedAssemblyPath; }
      
      set {
        if (selectedAssemblyPath != value) {
          selectedAssemblyPath = value;
          RaisePropertyChangedEvent(vm => vm.selectedAssemblyPath);
        }
      }

    }

    public string ServerAddresses {

      get { return serverAddresses; }

      set {
        if (serverAddresses != value) {
          serverAddresses = value;
          ServerAddressesArray = GetServerAddresses(serverAddresses);
          RaisePropertyChangedEvent(vm => vm.serverAddresses);
        }
      }
    }

    /// <summary>
    /// Splits a list of server addresses at every newline, removes empty entries,
    /// and returns the server addresses as array.
    /// </summary>
    /// <param name="serverAddresses">list of server addresses concatenated by newlines</param>
    /// <returns>array of server addresses, or an empty array if there are no server addresses</returns>
    private string[] GetServerAddresses(string serverAddresses) {
      return serverAddresses.Split(
                new string[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries
             );
    }

    public string[] ServerAddressesArray { get; private set; }

    private ICommand selectAssemblyCommand;
    public ICommand SelectAssemblyCommand {
      
      get {
        if (selectAssemblyCommand == null) {
          selectAssemblyCommand = new RelayCommand(param => {

            string filter = "DLL (*.dll)|*.dll|Executable (*.exe)|*.exe";

            OpenFileDialog openFileDialog
              = new OpenFileDialog() { Filter = filter, Multiselect = false };

            bool? result = openFileDialog.ShowDialog();
            if (result == true) {
              SelectedAssemblyPath = openFileDialog.FileName;
            }
          });
        }
        return selectAssemblyCommand;
      }

    }

    private ICommand deployAssemblyToServersCommand;
    public ICommand DeployAssemblyToServersCommand {

      get {
        if (deployAssemblyToServersCommand == null) {
          deployAssemblyToServersCommand = new RelayCommand(param => {

            foreach (string serverAddress in ServerAddressesArray) {
              logic.DeployAssemblyToServer(SelectedAssemblyPath, serverAddress);
            }
          });
        }
        return deployAssemblyToServersCommand;
      }

    }

    private ICommand startTestsCommand;
    public ICommand StartTestsCommand {
      
      get {
        if (startTestsCommand == null) {
          startTestsCommand = new RelayCommand(param => {
            // TODO
            Console.WriteLine("Start called.");
          });
        }
        return startTestsCommand;
      }

    }
  }
}
