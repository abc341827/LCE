namespace LCE.Models;

public sealed class MemoryScanResult
{
    public required string Address { get; init; }

    public required string Value { get; init; }

    public required string PreviousValue { get; init; }

    public required string ValueType { get; init; }

    public required string RegionBaseAddress { get; init; }

    public required string RegionSize { get; init; }

    public required string Protect { get; init; }
}
