using System.Diagnostics;

namespace LCE.Models;

public sealed class ProcessSnapshot
{
    public ProcessSnapshot(Process process)
    {
        Id = process.Id;
        Name = process.ProcessName;

        try
        {
            Path = process.MainModule?.FileName ?? string.Empty;
        }
        catch
        {
            Path = string.Empty;
        }
    }

    public int Id { get; }

    public string Name { get; }

    public string Path { get; }
}
