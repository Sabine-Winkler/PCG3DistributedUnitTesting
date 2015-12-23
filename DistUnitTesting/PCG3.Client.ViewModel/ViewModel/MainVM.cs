using Microsoft.Win32;
using PCG3.Client.Logic;
using PCG3.TestFramework;
using PCG3.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace PCG3.Client.ViewModel.ViewModel {

  /// <summary>
  /// Main view model of the client application.
  /// Binds data to the GUI, updates the GUI, and
  /// calls the client's logic.
  /// </summary>
  public class MainVM : ViewModelBase<MainVM> {

    #region message templates
    private const string TEMPLATE_DISTRIBUTED_TESTS = "[Client/MaiVM] Distributed tests to servers.";
    private const string TEMPLATE_UPDATED_TEST      = "[Client/MaiVM] Updated test {0}";
    #endregion

    private string selectedAssemblyPath;
    private string serverAddresses;
    private bool validServerAddresses;
    private bool validAssemblyPath;

    // to control if the 'Start' button is enabled/disabled
    private bool validServersAndAssembly;
    private Boolean TestsInProgress { get; set; }

    private ObservableCollection<Test> testColl;
    private ClientLogic logic;

    public Dispatcher GuiThreadDispatcher { get; set; }


    public MainVM(string assembly, string serverAddresses, Dispatcher dispatcher) {
      logic = new ClientLogic();

      if (assembly != "") {
        SelectedAssemblyPath = assembly;
      }

      if (serverAddresses != "") {
        ServerAddresses = serverAddresses;
      }

      TestsInProgress = false;
      GuiThreadDispatcher = dispatcher;
    }


    public string SelectedAssemblyPath {
      
      get { return selectedAssemblyPath; }
      
      set {
        if (selectedAssemblyPath != value) {
          selectedAssemblyPath = value;
          ValidAssemblyPath = (selectedAssemblyPath != null && selectedAssemblyPath != "");
          TestList = logic.GetTestMethodsOfAssembly(selectedAssemblyPath);
          TestColl = new ObservableCollection<Test>(TestList);
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
          UpdateStartButton();
          RaisePropertyChangedEvent(vm => vm.validServerAddresses);
        }
      }
    }


    public void UpdateStartButton() {
      ValidServersAndAssembly = ValidServerAddresses && ValidAssemblyPath && !TestsInProgress;
    }

    public bool ValidAssemblyPath {
      get { return validAssemblyPath; }

      set {
        if (validAssemblyPath != value) {
          validAssemblyPath = value;
          UpdateStartButton();
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


    private List<Test> TestList { get; set; }


    public ObservableCollection<Test> TestColl {
      get { return testColl; }

      set {
        if (testColl != value) {
          testColl = value;
          RaisePropertyChangedEvent(vm => vm.testColl);
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

            if (openFileDialog.ShowDialog() == true) {
              SelectedAssemblyPath = openFileDialog.FileName;              
            }
          });
        }
        return selectAssemblyCommand;
      }

    }

    private void OnTestCompleted(Test updTest) {
      
      string updTestMethodName = updTest.MethodName;
      Type updTestType = updTest.Type;

      for (int i = 0; i < TestColl.Count; ++i) {
        
        Test currTest = TestColl[i];

        if (currTest.MethodName.Equals(updTestMethodName)
              && currTest.Type.Equals(updTestType)) {
          
          GuiThreadDispatcher.Invoke(() => {
            // update the observable collection of tests on the GUI thread
            TestColl.RemoveAt(i);
            TestColl.Insert(i, updTest);
          });
          
          string message = string.Format(TEMPLATE_UPDATED_TEST, updTest);
          Logger.Log(message);
          
          break;
        }
      }
    }

    private ICommand startTestsCommand;
    public ICommand StartTestsCommand {
      
      get {
        if (startTestsCommand == null) {
          startTestsCommand = new RelayCommand(param => {
            
            // disable 'Start' button
            TestsInProgress = true;
            UpdateStartButton();

            Task.Run(() => {

              // step 1 - deploy assembly to the servers
              logic.DeployAssemblyToServers(SelectedAssemblyPath, ServerAddressesArray);

              // step 2 - distribute tests to the servers
              logic.DistributeTestsToServers(TestList, ServerAddressesArray, OnTestCompleted);

              Logger.Log(TEMPLATE_DISTRIBUTED_TESTS);

            }).ContinueWith((t) => {

              // enable 'Start' button, if possible
              TestsInProgress = false;
              UpdateStartButton();
            });
         });
        }
        return startTestsCommand;
      }

    }
  }
}