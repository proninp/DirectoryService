using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations.Requests;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Entities.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.CreateLocation;

public sealed partial class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationRepository _locationRepository;

    private readonly ITransactionManager _transactionManager;

    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(
        ILocationRepository locationRepository,
        ITransactionManager transactionManager,
        ILogger<CreateLocationHandler> logger)
    {
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Location '{@Request}' created with id '{@Id}'")]
    private static partial void LogLocationCreated(ILogger logger, CreateLocationRequest request, Guid id);

    public async Task<Result<Guid, Errors>> Handle(
        CreateLocationCommand command,
        CancellationToken cancellationToken)
    {
        var addressResult = command.Request.AddressRequest.ToAddress();

        var locationWithTheSameAddress = await _locationRepository.GetByAddress(addressResult.Value, cancellationToken);
        if (locationWithTheSameAddress is not null)
        {
            _logger.LogWarning(
                "Location with the same address already exists: {LocationAddress}", addressResult.Value);
            return Result.Failure<Guid, Errors>(GeneralErrors.AlreadyExists(
                locationWithTheSameAddress.Id,
                $"Location address already exists: '{command.Request.AddressRequest}' for " +
                $"location: '{locationWithTheSameAddress.Id}'",
                nameof(command.Request.AddressRequest)));
        }

        var locationWithTheSameName =
            await _locationRepository.GetByName(command.Request.Name, cancellationToken);

        if (locationWithTheSameName is not null)
        {
            _logger.LogWarning("Location with the same name already exists: {LocationId}", locationWithTheSameName.Id);
            return Result.Failure<Guid, Errors>(GeneralErrors.AlreadyExists(
                locationWithTheSameName.Id,
                $"Location with the name '{command.Request.Name}' already exists.",
                nameof(command.Request.Name)));
        }

        var locationResult = Location.Create(
            command.Request.Name,
            addressResult.Value,
            Timezone.Create(command.Request.Timezone).Value
        );

        if (locationResult.IsFailure)
        {
            _logger.LogWarning(
                "Failed to create location from request {@CreateLocationRequest}: {@Error}",
                command.Request, locationResult.Error);
            return locationResult.Error;
        }

        var id = _locationRepository.Add(locationResult.Value);

        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Error;

        LogLocationCreated(_logger, command.Request, id);

        return id;
    }
}