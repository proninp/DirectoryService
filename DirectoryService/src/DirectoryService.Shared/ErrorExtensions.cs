namespace DirectoryService.Shared;

public static class ErrorExtensions
{
    /// <summary>
    /// Компилятор не разворачивает generic-обёртки чтобы применить implicit operator внутри
    /// UnitResult&lt;Errors&gt; result = UnitResult.Failure&lt;Error&gt;(...); // не работает.
    /// </summary>
    /// <param name="error">Экземпляр Error.</param>
    /// <returns>Готовая обёртка Errors.</returns>
    public static Errors ToErrors(this Error error) => new(error);
}