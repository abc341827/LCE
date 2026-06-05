using System.ComponentModel;
using System.Runtime.InteropServices;
using LCE.Models;
using LCE.Native;

namespace LCE.Services;

public static class MemoryInspector
{
    public static IReadOnlyList<MemoryRegionInfo> EnumerateMemoryRegions(int processId)
    {
        using var processHandle = NativeMethods.OpenProcess(
            NativeMethods.ProcessQueryInformation | NativeMethods.ProcessVmRead,
            false,
            processId);

        if (processHandle.IsInvalid)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        NativeMethods.GetNativeSystemInfo(out var systemInfo);

        var regions = new List<MemoryRegionInfo>();
        var address = systemInfo.MinimumApplicationAddress;
        var maximumAddress = systemInfo.MaximumApplicationAddress;
        var informationSize = (nuint)Marshal.SizeOf<NativeMethods.MemoryBasicInformation>();

        while ((nuint)address < (nuint)maximumAddress)
        {
            var result = NativeMethods.VirtualQueryEx(processHandle, address, out var information, informationSize);
            if (result == 0)
            {
                break;
            }

            var baseAddress = (nuint)information.BaseAddress;
            var regionSize = information.RegionSize;
            var endAddress = baseAddress + regionSize;

            regions.Add(new MemoryRegionInfo
            {
                BaseAddress = FormatAddress(baseAddress),
                EndAddress = FormatAddress(endAddress),
                RegionSize = FormatSize(regionSize),
                State = FormatState(information.State),
                Protect = FormatProtect(information.Protect),
                Type = FormatType(information.Type),
                IsReadable = IsReadable(information.State, information.Protect)
            });

            if (endAddress <= (nuint)address)
            {
                break;
            }

            address = (nint)endAddress;
        }

        return regions;
    }

    private static bool IsReadable(uint state, uint protect)
    {
        if (state != NativeMethods.MemCommit)
        {
            return false;
        }

        if ((protect & NativeMethods.PageGuard) != 0 || (protect & NativeMethods.PageNoAccess) != 0)
        {
            return false;
        }

        var baseProtect = protect & 0xff;
        return baseProtect is NativeMethods.PageReadonly
            or NativeMethods.PageReadwrite
            or NativeMethods.PageWritecopy
            or NativeMethods.PageExecuteRead
            or NativeMethods.PageExecuteReadwrite
            or NativeMethods.PageExecuteWritecopy;
    }

    private static string FormatAddress(nuint address)
    {
        return $"0x{address:X}";
    }

    private static string FormatSize(nuint size)
    {
        const double kib = 1024d;
        const double mib = kib * 1024d;
        const double gib = mib * 1024d;

        var value = (double)size;
        return value switch
        {
            >= gib => $"{value / gib:N2} GiB",
            >= mib => $"{value / mib:N2} MiB",
            >= kib => $"{value / kib:N2} KiB",
            _ => $"{size} B"
        };
    }

    private static string FormatState(uint state)
    {
        return state switch
        {
            NativeMethods.MemCommit => "MEM_COMMIT",
            NativeMethods.MemReserve => "MEM_RESERVE",
            NativeMethods.MemFree => "MEM_FREE",
            _ => $"0x{state:X}"
        };
    }

    private static string FormatType(uint type)
    {
        return type switch
        {
            0 => string.Empty,
            NativeMethods.MemPrivate => "MEM_PRIVATE",
            NativeMethods.MemMapped => "MEM_MAPPED",
            NativeMethods.MemImage => "MEM_IMAGE",
            _ => $"0x{type:X}"
        };
    }

    private static string FormatProtect(uint protect)
    {
        if (protect == 0)
        {
            return string.Empty;
        }

        var baseProtect = protect & 0xff;
        var result = baseProtect switch
        {
            NativeMethods.PageNoAccess => "PAGE_NOACCESS",
            NativeMethods.PageReadonly => "PAGE_READONLY",
            NativeMethods.PageReadwrite => "PAGE_READWRITE",
            NativeMethods.PageWritecopy => "PAGE_WRITECOPY",
            NativeMethods.PageExecute => "PAGE_EXECUTE",
            NativeMethods.PageExecuteRead => "PAGE_EXECUTE_READ",
            NativeMethods.PageExecuteReadwrite => "PAGE_EXECUTE_READWRITE",
            NativeMethods.PageExecuteWritecopy => "PAGE_EXECUTE_WRITECOPY",
            _ => $"0x{baseProtect:X}"
        };

        if ((protect & NativeMethods.PageGuard) != 0)
        {
            result += " | PAGE_GUARD";
        }

        if ((protect & NativeMethods.PageNocache) != 0)
        {
            result += " | PAGE_NOCACHE";
        }

        if ((protect & NativeMethods.PageWriteCombine) != 0)
        {
            result += " | PAGE_WRITECOMBINE";
        }

        return result;
    }
}
