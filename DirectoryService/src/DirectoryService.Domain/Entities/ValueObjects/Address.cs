namespace DirectoryService.Domain.Entities.ValueObjects;

public sealed record Address
{
    public string? PostalCode { get; init; } // 10 symb

    public string? Country { get; init; } // 150 symb

    public string? City { get; init; } // 150 symb

    public string? Street { get; init; } // 150 symb

    public string? House { get; init; } // 50 symb

    public string? Block { get; init; } // 50 symb

    public string? Room { get; init; } // 50 symb

    public string? PostalBox { get; init; } // 50 symb
}