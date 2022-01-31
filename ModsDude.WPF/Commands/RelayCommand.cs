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
    private readonly Action<Exception> _exceptionHandler;


    public RelayCommand(Action action, Action<Exception> exceptionHandler)
    {
        _action = action;
        _exceptionHandler = exceptionHandler;
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
        try
        {
            _action();
        }
        catch (Exception ex)
        {
            _exceptionHandler(ex);
        }
    }
}
