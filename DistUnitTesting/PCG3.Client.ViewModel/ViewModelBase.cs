using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace PCG3.Client.ViewModel {

  /// <summary>
  /// Base class for view models, which implements the interface
  /// INotifyPropertyChanged.
  /// </summary>
  /// <typeparam name="TViewModel"></typeparam>
  public abstract class ViewModelBase<TViewModel> : INotifyPropertyChanged {

    public event PropertyChangedEventHandler PropertyChanged;


    protected void RaisePropertyChangedEvent<TProperty>(
                              Expression<Func<TViewModel, TProperty>> propertySelector) {

      if (PropertyChanged != null) {
        var propertyExpression = propertySelector.Body as MemberExpression;
        var propertyName = propertyExpression.Member.Name;

        if (PropertyChanged != null)
          PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
    }
  }

}
