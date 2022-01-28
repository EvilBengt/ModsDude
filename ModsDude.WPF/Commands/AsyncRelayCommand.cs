using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModsDude.WPF.Commands;

internal class AsyncRelayCommand : ICommand
{
    private readonly Func<object?, Task> _action;
    private readonly Action<Exception> _exceptionHandler;
    private bool _isExecuting;


    public AsyncRelayCommand(Func<Task> action, Action<Exception> exceptionHandler)
    {
        _action = (object? arg) => action();
        _exceptionHandler = exceptionHandler;
    }

    public AsyncRelayCommand(Func<object?, Task> action, Action<Exception> exceptionHandler)
    {
        _action = action;
        _exceptionHandler = exceptionHandler;
    }


    public Func<bool>? CanExecuteDelegate { get; set; }


    public event EventHandler? CanExecuteChanged;


    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, new());
    }

    public bool CanExecute(object? parameter)
    {
        return !_isExecuting && (CanExecuteDelegate?.Invoke() ?? true);
    }

    public async void Execute(object? parameter)
    {
        try
        {
            _isExecuting = true;
            OnCanExecuteChanged();
            await ExecuteAsync(parameter);
        }
        catch (Exception ex)
        {
            _exceptionHandler(ex);
        }
        finally
        {
            _isExecuting = false;
            OnCanExecuteChanged();
        }
    }

    public Task ExecuteAsync(object? parameter = null)
    {
        return _action(parameter);
    }
}
