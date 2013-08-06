using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Downloader.MVVM
{
    public class RelayCommand : ICommand
    {
        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return canExecuteDelegate == null ? true : canExecuteDelegate(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { System.Windows.Input.CommandManager.RequerySuggested += value; }
            remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            if (executeDelegate != null)
                executeDelegate(parameter);
        }

        #endregion

        private Action<object> executeDelegate;
        private Predicate<object> canExecuteDelegate;

        public RelayCommand(Action<object> executeDelegate, Predicate<object> canExecuteDelegate = null)
        {
            if (executeDelegate == null)
                throw new ArgumentNullException("executeDelegate");

            this.executeDelegate = executeDelegate;
            this.canExecuteDelegate = canExecuteDelegate;
        }
    }
}
