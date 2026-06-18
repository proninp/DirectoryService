using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments.UpdateDepartment;

public record UpdateDepartmentCommand(Guid Id, UpdateDepartmentRequest Request) : ICommand;

public static class UpdateDepartmentCommandExtensions
{
    public static UpdateDepartmentCommand ToCommand(this UpdateDepartmentRequest request, Guid id) =>
        new(id, request);
}