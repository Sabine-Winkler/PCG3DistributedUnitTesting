using Microsoft.Win32;
using PCG3.Client.Logic;
using PCG3.TestFramework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PCG3.Client.ViewModel.ViewModel {

  public class MainVM : ViewModelBase<MainVM> {

    private string selectedAssemblyPath;
    private string serverAddresses;
    private bool validServerAddresses;
    private bool validAssemblyPath;
    private bool validServersAndAssembly; // for 'Start' button
    private ObservableCollection<TestResult> results;
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
          ServerAddressesArray = SplitServerAddresses(serverAddresses);
          ValidServerAddresses = (serverAddresses != null && serverAddresses.Length > 0);
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
    private string[] SplitServerAddresses(string serverAddresses) {
      return serverAddresses.Split(
                new string[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries
             );
    }

    public string[] ServerAddressesArray { get; private set; }

    public bool ValidServerAddresses {
      get { return validServerAddresses; }

      set {
        if (validServerAddresses != value) {
          validServerAddresses = value;
          ValidServersAndAssembly = ValidServerAddresses && ValidAssemblyPath;
          RaisePropertyChangedEvent(vm => vm.validServerAddresses);
        }
      }
    }
    
    public bool ValidAssemblyPath {
      get { return validAssemblyPath; }

      set {
        if (validAssemblyPath != value) {
          validAssemblyPath = value;
          ValidServersAndAssembly = ValidServerAddresses && ValidAssemblyPath;
          RaisePropertyChangedEvent(vm => vm.validAssemblyPath);
        }
      }
    }

    public bool ValidServersAndAssembly {
      get { return validServersAndAssembly; }

      set {
        if (validServersAndAssembly != value) {
          validServersAndAssembly = value;
          RaisePropertyChangedEvent(vm => vm.validServersAndAssembly);
        }
      }
    }

    private List<TestResult> ResultList { get; set; }

    public ObservableCollection<TestResult> Results {
      get { return results; }

      set {
        if (results != value) {
          results = value;
          RaisePropertyChangedEvent(vm => vm.results);
        }
      }
    }

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
              ValidAssemblyPath = (SelectedAssemblyPath != null && SelectedAssemblyPath != "");

              ResultList = logic.AssemblyToList(SelectedAssemblyPath);
              Results
                = new ObservableCollection<TestResult>(ResultList);
              
            }
          });
        }
        return selectAssemblyCommand;
      }

    }

    private ICommand startTestsCommand;
    public ICommand StartTestsCommand {
      
      get {
        if (startTestsCommand == null) {
          startTestsCommand = new RelayCommand(param => {
            
            foreach (string serverAddress in ServerAddressesArray) {

              // step 1 - deploy assembly to the servers
              logic.DeployAssemblyToServer(SelectedAssemblyPath, serverAddress);
            }

            // step 2 - send tests to the servers
            logic.SendTestsToServer(ResultList, ServerAddressesArray);

          });
        }
        return startTestsCommand;
      }

    }
  }
}
