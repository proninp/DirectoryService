using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments.CreateDepartment;

public record CreateDepartmentCommand(CreateDepartmentRequest Request) : IValidationCommand
{
    public object LogContext => Request;
}

public static class CreateDepartmentCommandExtensions
{
    public static CreateDepartmentCommand ToCreateCommand(this CreateDepartmentRequest request) =>
        new(request);
}