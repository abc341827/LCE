namespace LCE.Models;

public sealed class MemoryScanSession
{
    public MemoryScanSession(int processId, ScanValueType valueType, int valueByteLength, string resultFilePath, long resultCount, bool usedPrivateReadWriteOnly, string diagnostics)
    {
        ProcessId = processId;
        ValueType = valueType;
        ValueByteLength = valueByteLength;
        ResultFilePath = resultFilePath;
        ResultCount = resultCount;
        UsedPrivateReadWriteOnly = usedPrivateReadWriteOnly;
        Diagnostics = diagnostics;
    }

    public int ProcessId { get; }

    public ScanValueType ValueType { get; }

    public int ValueByteLength { get; }

    public string ResultFilePath { get; }

    public long ResultCount { get; }

    public bool UsedPrivateReadWriteOnly { get; }

    public string Diagnostics { get; }
}
