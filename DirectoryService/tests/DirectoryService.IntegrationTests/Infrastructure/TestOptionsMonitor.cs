using Microsoft.Extensions.Options;

namespace DirectoryService.IntegrationTests.Infrastructure;

/// <summary>
/// Монитор опций, значение которого вычисляется на каждое чтение по его порядковому номеру (с 1).
/// Позволяет эмулировать hot reload и невалидный конфиг без файлов конфигурации.
/// Если передан валидатор, ведёт себя как настоящий OptionsMonitor: невалидное значение
/// приводит к <see cref="OptionsValidationException"/> при чтении.
/// </summary>
internal sealed class TestOptionsMonitor<T> : IOptionsMonitor<T>
    where T : class
{
    private readonly Func<int, T> _valueFactory;

    private readonly IValidateOptions<T>? _validator;

    private int _readCount;

    public TestOptionsMonitor(T value, IValidateOptions<T>? validator = null)
        : this(_ => value, validator)
    {
    }

    public TestOptionsMonitor(Func<int, T> valueFactory, IValidateOptions<T>? validator = null)
    {
        _valueFactory = valueFactory;
        _validator = validator;
    }

    public int ReadCount => Volatile.Read(ref _readCount);

    public T CurrentValue => Get(Options.DefaultName);

    public T Get(string? name)
    {
        var value = _valueFactory(Interlocked.Increment(ref _readCount));
        if (_validator is null)
            return value;

        var optionsName = name ?? Options.DefaultName;
        var result = _validator.Validate(optionsName, value);
        if (result.Failed)
            throw new OptionsValidationException(optionsName, typeof(T), result.Failures);

        return value;
    }

    public IDisposable? OnChange(Action<T, string?> listener) => null;
}
