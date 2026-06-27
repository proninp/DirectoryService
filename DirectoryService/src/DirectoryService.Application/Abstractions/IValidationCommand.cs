namespace DirectoryService.Application.Abstractions;

public interface IValidationCommand : ICommand
{
    public object? LogContext { get; }
}