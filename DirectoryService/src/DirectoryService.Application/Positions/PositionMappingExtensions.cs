using DirectoryService.Contracts.Positions.Responses;
using DirectoryService.Domain.Entities;

namespace DirectoryService.Application.Positions;

public static class PositionMappingExtensions
{
    public static PositionResponse ToResponse(this Position position)
    {
        return new PositionResponse(
            position.Id,
            position.Name,
            position.Description,
            [.. position.DepartmentPositions.Select(dp => dp.DepartmentId)]);
    }
}