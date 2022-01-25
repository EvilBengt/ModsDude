using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModsDude.WPF.Commands;

internal class RelayCommand : ICommand
{
    private readonly Action _action;


    public RelayCommand(Action action)
    {
        _action = action;
    }


    public event EventHandler? CanExecuteChanged;


    public Func<bool>? CanExecuteDelegate { get; set; }

    
    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, new());
    }

    public bool CanExecute(object? parameter)
    {
        return CanExecuteDelegate?.Invoke() ?? true;
    }

    public void Execute(object? parameter)
    {
        _action();
    }
}
