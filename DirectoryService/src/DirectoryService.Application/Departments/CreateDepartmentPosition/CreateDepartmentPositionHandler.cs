using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Positions;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.CreateDepartmentPosition;

public sealed class CreateDepartmentPositionHandler
    : ICommandHandler<DepartmentPositionResponse, CreateDepartmentPositionCommand>
{
    private readonly IDepartmentRepository _departmentRepository;

    private readonly IPositionRepository _positionRepository;

    private readonly ITransactionManager _transactionManager;

    private readonly ILogger<CreateDepartmentPositionHandler> _logger;

    public CreateDepartmentPositionHandler(
        IDepartmentRepository departmentRepository,
        IPositionRepository positionRepository,
        ITransactionManager transactionManager,
        ILogger<CreateDepartmentPositionHandler> logger)
    {
        _departmentRepository = departmentRepository;
        _positionRepository = positionRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<DepartmentPositionResponse, Errors>> Handle(
        CreateDepartmentPositionCommand command,
        CancellationToken cancellationToken)
    {
        var department = await _departmentRepository.GetById(command.DepartmentId, cancellationToken);

        if (department is null)
        {
            _logger.LogWarning(
                "Create department-position error. Department was not found by id: '{@DepartmentId}'.",
                command.DepartmentId);
            return GeneralErrors.NotFound(
                    recordName: nameof(Department), message: $"Department {command.DepartmentId} not found.")
                .ToErrors();
        }

        var departmentPosition = department.DepartmentPositions
            .FirstOrDefault(dp => dp.PositionId == command.PositionId);

        if (departmentPosition is not null)
        {
            _logger.LogWarning(
                "Create department-position error. Relation already exists " +
                $"for DepartmentId {command.DepartmentId} and PositionId {command.PositionId}.");
            return GeneralErrors.AlreadyExists(
                    departmentPosition.Id,
                    $"Department {command.DepartmentId} and Position {command.PositionId} relation already exists")
                .ToErrors();
        }

        var position = await _positionRepository.GetById(command.PositionId, cancellationToken);
        if (position is null)
        {
            _logger.LogError(
                $"Create department-position error. Position was not found by id: '{command.PositionId}'.");
            return GeneralErrors.NotFound(
                    recordName: nameof(Position), message: $"Position {command.PositionId} not found.")
                .ToErrors();
        }

        department.AddPosition(command.PositionId);

        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            if (saveChangesResult.Error.IsUniqueViolation())
            {
                return GeneralErrors.AlreadyExists(
                        message:
                        $"Department {command.DepartmentId} and Position {command.PositionId} relation already exists.")
                    .ToErrors();
            }

            return saveChangesResult.Error;
        }

        return department.ToDepartmentPositionResponse();
    }
}
