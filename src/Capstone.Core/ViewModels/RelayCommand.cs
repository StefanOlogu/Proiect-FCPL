using System.Windows.Input;

namespace Capstone.Core.ViewModels;

/// <summary>
/// A button's behaviour as two functions: what it does, and whether it can right now.
/// <see cref="ICommand"/> lives in System.ObjectModel, not in a UI framework, so the core library can use it.
/// </summary>
public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool> _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(execute);
        _execute = execute;
        _canExecute = canExecute ?? (_ => true);
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute(parameter);

    /// <summary>Does nothing when it can't: bindings don't always ask first.</summary>
    public void Execute(object? parameter)
    {
        if (CanExecute(parameter)) _execute(parameter);
    }

    /// <summary>Tells the button to ask <see cref="CanExecute"/> again.</summary>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
