using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Positions.Responses;
using DirectoryService.Domain.Entities;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Positions.GetPosition;

public sealed class GetPositionHandler(
    IReadDbContext context,
    ILogger<GetPositionHandler> logger
) : IQueryHandler<PositionResponse, GetPositionQuery>
{
    public async Task<Result<PositionResponse, Errors>> Handle(
        GetPositionQuery query, CancellationToken cancellationToken)
    {
        if (query.Id == Guid.Empty)
        {
            logger.LogWarning($"Get {nameof(Position)} query error: query id parameter is empty.");
            return GeneralErrors.ValueIsRequired(nameof(query.Id)).ToErrors();
        }

        var positionResponse = await context.PositionsQuery
            .Where(p => p.Id == query.Id)
            .Select(PositionMappingExtensions.ToResponseExpression())
            .FirstOrDefaultAsync(cancellationToken);

        if (positionResponse == null)
        {
            logger.LogWarning(
                "{PositionQueryName} query error: position was not found with id: {PositionId}.",
                nameof(GetPositionQuery),
                query.Id);
            return GeneralErrors.NotFound(
                    query.Id, nameof(Position), $"No active positions were found with id: '{query.Id}'.")
                .ToErrors();
        }

        return positionResponse;
    }
}