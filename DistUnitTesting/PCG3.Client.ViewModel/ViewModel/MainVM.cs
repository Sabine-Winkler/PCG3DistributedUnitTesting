using Microsoft.Win32;
using PCG3.Client.Logic;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace PCG3.Client.ViewModel.ViewModel {

  public class MainVM : ViewModelBase<MainVM> {

    private string selectedAssemblyPath;
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

    private ICommand selectAssemblyCommand;
    public ICommand SelectAssemblyCommand {
      
      get {
        if (selectAssemblyCommand == null) {
          selectAssemblyCommand = new RelayCommand(param => {

            string filter = "Executable (*.exe)|*.exe|DLL (*.dll)|*.dll";

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
            
            // TODO: fetch from GUI
            List<string> serverAddresses
              = new List<string>() { "localhost:9000" };

            foreach (string serverAddress in serverAddresses) {
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
