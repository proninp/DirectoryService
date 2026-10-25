using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Decorators;

internal static class CommandValidation
{
    public static async Task<Errors?> ValidateAsync<TCommand>(
        IEnumerable<IValidator<TCommand>> validators,
        TCommand command,
        ILogger logger,
        CancellationToken cancellationToken)
        where TCommand : IValidationCommand
    {
        var iValidators = validators as IValidator<TCommand>[] ?? validators.ToArray();
        if (iValidators.Length == 0)
            return null;

        var context = new ValidationContext<TCommand>(command);
        var validationResults = await Task.WhenAll(
            iValidators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var validationErrors = validationResults
            .Where(r => !r.IsValid)
            .ToList();

        if (validationErrors.Count == 0)
            return null;

        var errors = validationErrors.ToErrors();
        logger.LogWarning(
            "Validation failed for {CommandType}: {@Context} with errors: {@Errors}",
            typeof(TCommand).Name,
            command.LogContext,
            errors);
        return errors;
    }
}
