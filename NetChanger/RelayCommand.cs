using System;
using System.Windows.Input;

namespace NetChanger
{
    public class RelayCommand : ICommand
    {
        private Action<object> _execute;
        private Func<object, bool> _canExecute;

        protected RelayCommand() { }

        private RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? (p => true);
        }

        public RelayCommand(Action execute) : this(p => execute(), (Func<object, bool>)null) { }

        public RelayCommand(Action execute, Func<bool> canExecute) : this(p => execute(), p => canExecute()) { }

        public RelayCommand(Action<object> execute, Func<bool> canExecute) : this(execute, p => canExecute()) { }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }
    }
}