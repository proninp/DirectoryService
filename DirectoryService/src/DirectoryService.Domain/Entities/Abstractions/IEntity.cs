namespace DirectoryService.Domain.Entities.Abstractions;

/// <summary>
/// Маркерный интерфейс для сущности, хранимой в базе.
/// </summary>
public interface IEntity;

/// <summary>
/// Сущность, хранимая в базе, с первичным ключом.
/// </summary>
/// <typeparam name="TKey">Тип первичного ключа.</typeparam>
public interface IEntity<TKey> : IEntity
    where TKey : struct, IEquatable<TKey>
{
    /// <summary>
    /// Первичный ключ сущности.
    /// </summary>
    TKey Id { get; init; }
}