using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Application.Abstractions.Database;

public interface ITransactionScope : IDisposable
{
    UnitResult<Errors> Commit();

    UnitResult<Errors> Rollback();
}