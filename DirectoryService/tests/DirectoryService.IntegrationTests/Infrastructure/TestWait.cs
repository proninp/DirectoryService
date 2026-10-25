namespace DirectoryService.IntegrationTests.Infrastructure;

internal static class TestWait
{
    private static readonly TimeSpan s_pollInterval = TimeSpan.FromMilliseconds(50);

    public static async Task UntilAsync(Func<bool> condition, TimeSpan timeout, string description)
    {
        using var cts = new CancellationTokenSource(timeout);
        while (!condition())
        {
            try
            {
                await Task.Delay(s_pollInterval, cts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException($"Condition was not met within {timeout}: {description}");
            }
        }
    }
}
