using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments.UpdateDepartment;

public record UpdateDepartmentCommand(UpdateDepartmentRequest Request) : ICommand;

public static class UpdateDepartmentCommandExtensions
{
    public static UpdateDepartmentCommand ToUpdateCommand(this UpdateDepartmentRequest request) =>
        new(request);
}