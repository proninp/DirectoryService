using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.UpdateDepartment;

public sealed class UpdateDepartmentHandler : ICommandHandler<DepartmentResponse, UpdateDepartmentCommand>
{
    private readonly IDepartmentRepository _repository;

    private readonly ITransactionManager _transactionManager;

    private readonly ILogger<UpdateDepartmentHandler> _logger;

    public UpdateDepartmentHandler(
        ITransactionManager transactionManager,
        IDepartmentRepository repository,
        ILogger<UpdateDepartmentHandler> logger)
    {
        _transactionManager = transactionManager;
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<DepartmentResponse, Errors>> Handle(
        UpdateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var department = await _repository.GetById(command.Id, cancellationToken);
        if (department is null)
        {
            _logger.LogWarning(
                "Update department error. Department was not found by id: '{DepartmentId}'.",
                command.Id);
            return GeneralErrors.NotFound(
                    recordName: nameof(Department), message: $"Department {command.Id} not found.")
                .ToErrors();
        }

        var renameResult = department.Rename(command.Request.Name);
        if (renameResult.IsFailure)
        {
            _logger.LogWarning(
                "Department rename error. Cannot rename the department {DepartmentId} with a new name: '{DepartmentName}'.",
                command.Id, command.Request.Name);
            return renameResult.Error;
        }

        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            return saveChangesResult.Error;
        }

        return department.ToResponse();
    }
}