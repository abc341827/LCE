namespace LCE.Models;

public sealed class MemoryRegionInfo
{
    public required string BaseAddress { get; init; }

    public required string EndAddress { get; init; }

    public required string RegionSize { get; init; }

    public required string State { get; init; }

    public required string Protect { get; init; }

    public required string Type { get; init; }

    public bool IsReadable { get; init; }
}
