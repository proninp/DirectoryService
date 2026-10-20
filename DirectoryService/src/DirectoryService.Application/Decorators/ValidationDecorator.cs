using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
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
        var errors = await CommandValidation.ValidateAsync(_validators, command, _logger, cancellationToken);
        if (errors is not null)
            return errors;

        return await _inner.Handle(command, cancellationToken);
    }
}

public sealed class ValidationDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : IValidationCommand
{
    private readonly IEnumerable<IValidator<TCommand>> _validators;

    private readonly ICommandHandler<TCommand> _inner;

    private readonly ILogger<ValidationDecorator<TCommand>> _logger;

    public ValidationDecorator(
        IEnumerable<IValidator<TCommand>> validators,
        ICommandHandler<TCommand> inner,
        ILogger<ValidationDecorator<TCommand>> logger)
    {
        _validators = validators;
        _inner = inner;
        _logger = logger;
    }

    public async Task<UnitResult<Errors>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var errors = await CommandValidation.ValidateAsync(_validators, command, _logger, cancellationToken);
        if (errors is not null)
            return UnitResult.Failure(errors);

        return await _inner.Handle(command, cancellationToken);
    }
}
