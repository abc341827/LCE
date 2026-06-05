namespace LCE.Models;

public sealed class MemoryScanSession
{
    public MemoryScanSession(int processId, ScanValueType valueType, int valueByteLength, IReadOnlyList<MemoryScanCandidate> candidates)
    {
        ProcessId = processId;
        ValueType = valueType;
        ValueByteLength = valueByteLength;
        Candidates = candidates.ToList();
    }

    public int ProcessId { get; }

    public ScanValueType ValueType { get; }

    public int ValueByteLength { get; }

    public List<MemoryScanCandidate> Candidates { get; }
}
