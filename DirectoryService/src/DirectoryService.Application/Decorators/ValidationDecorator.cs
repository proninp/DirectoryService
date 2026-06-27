using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Decorators;

public sealed class ValidationDecorator<TResponse, TCommand> : ICommandHandler<TResponse, TCommand>
    where TCommand : IValidationCommand
{
    private readonly IEnumerable<IValidator<TCommand>> _validators;

    private readonly ICommandHandler<TResponse, TCommand> _inner;

    private readonly ILogger<ValidationDecorator<TResponse, TCommand>> _logger;

    public ValidationDecorator(
        IEnumerable<IValidator<TCommand>> validators,
        ICommandHandler<TResponse, TCommand> inner,
        ILogger<ValidationDecorator<TResponse, TCommand>> logger)
    {
        _validators = validators;
        _inner = inner;
        _logger = logger;
    }

    public async Task<Result<TResponse, Errors>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await _inner.Handle(command, cancellationToken);

        var context = new ValidationContext<TCommand>(command);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var validationErrors = validationResults
            .Where(r => !r.IsValid)
            .ToList();

        if (validationErrors.Count > 0)
        {
            var errors = validationErrors.ToErrors();
            _logger.LogWarning(
                "Validation failed for {CommandType}: {@Context} with errors: {@Errors}",
                typeof(TCommand).Name,
                command.LogContext,
                errors);
            return errors;
        }

        return await _inner.Handle(command, cancellationToken);
    }
}