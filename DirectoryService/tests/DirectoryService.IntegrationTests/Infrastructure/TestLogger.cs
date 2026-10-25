using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace DirectoryService.IntegrationTests.Infrastructure;

internal sealed record TestLogEntry(
    LogLevel Level,
    string Message,
    IReadOnlyDictionary<string, object?> Properties,
    Exception? Exception);

internal sealed class TestLogger<T> : ILogger<T>
{
    private readonly ConcurrentQueue<TestLogEntry> _entries = new();

    public IReadOnlyCollection<TestLogEntry> Entries => _entries;

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var properties = state is IEnumerable<KeyValuePair<string, object?>> pairs
            ? pairs.ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal)
            : [];

        _entries.Enqueue(new TestLogEntry(logLevel, formatter(state, exception), properties, exception));
    }

    // Повторяющиеся записи (одна и та же ошибка на каждой итерации) схлопываются
    public string Dump() =>
        string.Join(
            Environment.NewLine,
            _entries
                .Select(e => e.Exception is null
                    ? $"[{e.Level}] {e.Message}"
                    : $"[{e.Level}] {e.Message} {e.Exception.GetType().Name}: {e.Exception.Message}")
                .Distinct(StringComparer.Ordinal));
}
