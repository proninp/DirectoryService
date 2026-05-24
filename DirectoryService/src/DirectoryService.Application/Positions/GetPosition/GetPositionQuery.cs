using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Positions.GetPosition;

public record GetPositionQuery(Guid Id) : IQuery;