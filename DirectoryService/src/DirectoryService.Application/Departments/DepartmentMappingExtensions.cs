using DirectoryService.Contracts.Departments.Responses;
using DirectoryService.Domain.Entities;

namespace DirectoryService.Application.Departments;

public static class DepartmentMappingExtensions
{
    public static DepartmentResponse ToResponse(this Department department)
    {
        return new DepartmentResponse(
            department.Id,
            department.Name,
            department.Slug.Value,
            department.ParentId,
            department.Path.Value,
            department.Depth,
            [.. department.DepartmentLocations.Select(dl => dl.LocationId)]);
    }
}