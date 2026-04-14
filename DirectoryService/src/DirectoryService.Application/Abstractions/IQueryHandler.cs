using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Application.Abstractions;

public interface IQueryHandler<TResponse, in TQuery>
    where TQuery : IQuery
{
    Task<Result<TResponse, Errors>> Handle(TQuery query, CancellationToken cancellationToken);
}