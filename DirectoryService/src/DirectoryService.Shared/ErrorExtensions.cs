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

    public static bool IsUniqueViolation(this Errors errors)
    {
        var errorsList = errors.ToList();
        return errorsList is
            [{ ErrorType: ErrorType.Conflict, ErrorMessage.Code: GeneralErrors.UniquenessViolationCode }];
    }

    public static bool IsConcurrencyConflict(this Errors errors)
    {
        var errorsList = errors.ToList();
        return errorsList is
            [{ ErrorType: ErrorType.Conflict, ErrorMessage.Code: GeneralErrors.ConcurrencyViolationCode }];
    }

    public static bool IsForeignKeyViolation(this Errors errors)
    {
        var errorsList = errors.ToList();
        return errorsList is
            [{ ErrorType: ErrorType.Validation, ErrorMessage.Code: GeneralErrors.ForeignKeyViolationCode }];
    }
}