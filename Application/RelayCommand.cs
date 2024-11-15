using System.Windows.Input;

namespace CAndrews.CameraFileManagement.Application;

/// <summary>
/// Defines a command
/// </summary>
/// <param name="execute">Data used by the command.
/// If the command does not require data to be passed, this object can be set to null.</param>
/// <param name="canExecute">Data used by the command.
/// If the command does not require data to be passed, this object can be set to null.</param>
internal class RelayCommand(Action<object> execute, Predicate<object> canExecute = default) : ICommand
{
    /// <inheritdoc/>
    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <inheritdoc/>
    public bool CanExecute(object parameter) => canExecute is null || canExecute(parameter);

    /// <inheritdoc/>
    public void Execute(object parameter) => execute(parameter);
}