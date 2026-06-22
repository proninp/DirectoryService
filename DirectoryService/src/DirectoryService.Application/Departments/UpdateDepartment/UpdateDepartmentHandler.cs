using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.UpdateDepartment;

public sealed class UpdateDepartmentHandler : ICommandHandler<DepartmentResponse, UpdateDepartmentCommand>
{
    private readonly IValidator<UpdateDepartmentCommand> _validator;

    private readonly IDepartmentRepository _repository;

    private readonly IUnitOfWork _unitOfWork;

    private readonly ILogger<UpdateDepartmentHandler> _logger;

    public UpdateDepartmentHandler(
        IValidator<UpdateDepartmentCommand> validator,
        IUnitOfWork unitOfWork,
        IDepartmentRepository repository,
        ILogger<UpdateDepartmentHandler> logger)
    {
        _validator = validator;
        _unitOfWork = unitOfWork;
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<DepartmentResponse, Errors>> Handle(
        UpdateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToErrors();
            _logger.LogWarning(
                "Failed to update department from request {@DepartmentRequest}: {@Error}",
                request, errors);
            return errors;
        }

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

        var renameResult = department.Rename(request.Name);
        if (renameResult.IsFailure)
        {
            _logger.LogWarning(
                "Department rename error. Cannot rename the department {DepartmentId} with a new name: '{DepartmentName}'.",
                command.Id, request.Name);
            return renameResult.Error;
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        return department.ToResponse();
    }
}