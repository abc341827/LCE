namespace LCE.Models;

public sealed class MemoryScanResult
{
    public required string Address { get; init; }

    public required string Value { get; init; }

    public required string ValueType { get; init; }
}
