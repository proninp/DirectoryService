using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.DeleteDepartmentPosition;

public sealed class DeleteDepartmentPositionHandler
    : ICommandHandler<DepartmentPositionResponse, DeleteDepartmentPositionCommand>
{
    private readonly IDepartmentRepository _departmentRepository;

    private readonly ITransactionManager _transactionManager;

    private readonly ILogger<DeleteDepartmentPositionHandler> _logger;

    public DeleteDepartmentPositionHandler(
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        ILogger<DeleteDepartmentPositionHandler> logger)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<DepartmentPositionResponse, Errors>> Handle(
        DeleteDepartmentPositionCommand command,
        CancellationToken cancellationToken)
    {
        var department = await _departmentRepository.GetById(command.DepartmentId, cancellationToken);

        if (department is null)
        {
            _logger.LogWarning(
                "Delete department-position error. Department was not found by id: '{DepartmentId}'.",
                command.DepartmentId);
            return GeneralErrors.NotFound(
                    recordName: nameof(Department), message: $"Department {command.DepartmentId} not found.")
                .ToErrors();
        }

        var departmentPosition = department.DepartmentPositions
            .FirstOrDefault(dp => dp.PositionId == command.PositionId);

        if (departmentPosition is null)
        {
            _logger.LogWarning(
                "Delete department-position error. Relation does not exist " +
                "for DepartmentId {@DepartmentId} and PositionId {@PositionId}.",
                command.DepartmentId, command.PositionId);
            return GeneralErrors.NotFound(
                    recordName: nameof(DepartmentPosition),
                    message:
                    $"Department {command.DepartmentId} and Position {command.PositionId} relation does not exist")
                .ToErrors();
        }

        var removeResult = department.RemovePosition(command.PositionId);
        if (removeResult.IsFailure)
        {
            _logger.LogWarning(
                "Remove department-position failed. Cannot remove position {PositionId} " +
                "from department {DepartmentId}.",
                command.PositionId, command.DepartmentId);
            return removeResult.Error;
        }

        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            return saveChangesResult.Error;
        }

        var commitResult = saveChangesResult.Value;
        if (commitResult == 0)
        {
            _logger.LogWarning(
                "Concurrent delete detected: DepartmentPosition for " +
                "DepartmentId {DepartmentId} and PositionId {PositionId} relation does not exist.",
                command.DepartmentId, command.PositionId);
            return GeneralErrors.NotFound(
                    message:
                    $"Department {command.DepartmentId} and Position {command.PositionId} relation does not exist.")
                .ToErrors();
        }

        return department.ToDepartmentPositionResponse();
    }
}