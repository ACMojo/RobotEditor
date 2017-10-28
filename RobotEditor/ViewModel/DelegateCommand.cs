using System;
using System.Windows.Input;

namespace RobotEditor.ViewModel
{
    internal class DelegateCommand<T> : ICommand
    {
        #region Events

        public event EventHandler CanExecuteChanged;

        #endregion

        #region Fields

        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        #endregion

        #region Instance

        public DelegateCommand(Action<T> execute)
            : this(execute, null) { }

        public DelegateCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion

        #region Public methods

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke((T)parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            _execute?.Invoke((T)parameter);
        }

        public void RaisePropertyChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}