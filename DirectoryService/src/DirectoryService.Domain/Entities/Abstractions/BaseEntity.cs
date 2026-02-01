namespace DirectoryService.Domain.Entities.Abstractions;

/// <summary>
/// Базовый абстрактный класс для сущностей с идентификатором.
/// </summary>
public abstract class BaseEntity : IEntity<Guid>
{
    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public Guid Id { get; init; }
}
