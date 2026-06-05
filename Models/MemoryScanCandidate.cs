namespace LCE.Models;

public sealed class MemoryScanCandidate
{
    public nuint Address { get; init; }

    public required byte[] PreviousBytes { get; set; }

    public required string PreviousValue { get; set; }

    public required string CurrentValue { get; set; }

    public required string RegionBaseAddress { get; init; }

    public required string RegionSize { get; init; }

    public required string Protect { get; init; }
}
