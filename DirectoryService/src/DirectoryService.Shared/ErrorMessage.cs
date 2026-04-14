namespace DirectoryService.Shared;

public record ErrorMessage(string Code, string Message, string? InvalidField = null);