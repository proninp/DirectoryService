using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments.Requests;

namespace DirectoryService.Application.Departments.GetDepartment;

public record GetDepartmentListQuery(GetDepartmentListRequest Request) : IQuery;

public static class GetDepartmentListQueryExtensions
{
    public static GetDepartmentListQuery ToQuery(this GetDepartmentListRequest request) =>
        new(request);
}