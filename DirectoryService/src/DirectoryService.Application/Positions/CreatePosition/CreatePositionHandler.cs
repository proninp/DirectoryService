using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Positions.CreatePosition;

public sealed class CreatePositionHandler(
    IPositionRepository positionRepository,
    IDepartmentRepository departmentRepository,
    ILogger<CreatePositionHandler> logger
) : ICommandHandler<Guid, CreatePositionCommand>
{
    public async Task<Result<Guid, Errors>> Handle(CreatePositionCommand command, CancellationToken cancellationToken)
    {
        var positionByName = await positionRepository.GetByName(command.Request.Name, cancellationToken);
        if (positionByName is not null && positionByName.IsActive)
        {
            logger.LogWarning(
                "Position creation error. Active position with name '{PositionName}' is already exists.",
                command.Request.Name);
            return GeneralErrors.AlreadyExists(
                    message: $"Active position with name '{command.Request.Name}' already exists.",
                    invalidField: nameof(command.Request.Name))
                .ToErrors();
        }

        var allDepartmentsExists =
            await departmentRepository.AllExists(command.Request.DepartmentIds, cancellationToken);

        if (!allDepartmentsExists)
        {
            logger.LogError(
                "Position creation error. One or several of departments not found: '{@DepartmentIds}'.",
                command.Request.DepartmentIds);
            return GeneralErrors.NotFound(
                    recordName: nameof(Position), message: "One or several departments not found.")
                .ToErrors();
        }

        var positionId = Guid.NewGuid();

        var departmentPositions =
            command.Request.DepartmentIds
                .Select(departmentId => new DepartmentPosition(departmentId, positionId))
                .ToList();

        var position = Position.Create(
            positionId, command.Request.Name, command.Request.Description, departmentPositions);

        await positionRepository.Add(position.Value, cancellationToken);
        return positionId;
    }
}