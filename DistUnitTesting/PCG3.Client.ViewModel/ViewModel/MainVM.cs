using Microsoft.Win32;
using System;
using System.Windows.Input;

namespace PCG3.Client.ViewModel.ViewModel {

  public class MainVM : ViewModelBase<MainVM> {

    private string selectedAssemblyPath;

    public MainVM() {
      // TODO
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
