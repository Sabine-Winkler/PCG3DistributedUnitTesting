using System;
using System.Windows.Input;

namespace PCG3.Client.ViewModel {

  /// <summary>
  /// RelayCommand objects delegate to the methods passed as an argument.
  /// </summary>
  public class RelayCommand : ICommand {

    readonly Action<object> executeAction;
    readonly Predicate<object> canExecutePredicate;

    public RelayCommand(Action<object> execute) : this(execute, null) { }

    public RelayCommand(Action<object> execute, Predicate<object> canExecute) {
      if (execute == null)
        throw new ArgumentNullException("execute");

      executeAction = execute;
      canExecutePredicate = canExecute;
    }

    /// <summary>
    /// Is invoked when command is executed 
    /// </summary>
    public void Execute(object parameter) {
      executeAction(parameter);
    }

    /// <summary>
    /// The returned value indicates whether command is active (can be executed) or not.
    /// </summary>
    public bool CanExecute(object parameter) {
      return canExecutePredicate == null ? true : canExecutePredicate(parameter);
    }

    /// <summary>
    /// Occurs when changes occur that affect whether or not the command should execute.
    /// The handling of this event is delegated to the CommandManager.
    /// WPF invokes CanExecute in response to this event. CanExecute returns the
    /// command's state. 
    /// </summary>
    public event EventHandler CanExecuteChanged {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }
  }
}
