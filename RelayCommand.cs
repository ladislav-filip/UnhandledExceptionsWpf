using System;
using System.Windows.Input;

namespace UnhandledExceptionsWpf
{
    public class RelayCommand : ICommand
    {
        private readonly Action _workToDo;

        public RelayCommand(Action workToDo)
        {
            _workToDo = workToDo;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _workToDo();
        }

        public event EventHandler CanExecuteChanged;
    }
}