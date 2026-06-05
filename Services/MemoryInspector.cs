using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using LCE.Models;
using LCE.Native;

namespace LCE.Services;

public static class MemoryInspector
{
    private const int ScanBufferSize = 1024 * 1024;

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
                StateDescription = DescribeState(information.State),
                Protect = FormatProtect(information.Protect),
                ProtectDescription = DescribeProtect(information.Protect),
                Type = FormatType(information.Type),
                TypeDescription = DescribeType(information.Type),
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

    public static MemoryScanSession FirstScan(int processId, ScanValueType valueType, string valueText, int maxResults = 10000)
    {
        var pattern = GetValueBytes(valueType, valueText);

        using var processHandle = OpenReadableProcess(processId);
        NativeMethods.GetNativeSystemInfo(out var systemInfo);

        var candidates = new List<MemoryScanCandidate>();
        var address = systemInfo.MinimumApplicationAddress;
        var maximumAddress = systemInfo.MaximumApplicationAddress;
        var informationSize = (nuint)Marshal.SizeOf<NativeMethods.MemoryBasicInformation>();

        while ((nuint)address < (nuint)maximumAddress && candidates.Count < maxResults)
        {
            var queryResult = NativeMethods.VirtualQueryEx(processHandle, address, out var information, informationSize);
            if (queryResult == 0)
            {
                break;
            }

            var baseAddress = (nuint)information.BaseAddress;
            var regionSize = information.RegionSize;
            var endAddress = baseAddress + regionSize;

            if (IsReadable(information.State, information.Protect))
            {
                ScanRegionForPattern(processHandle, baseAddress, regionSize, information.Protect, valueType, pattern, maxResults, candidates);
            }

            if (endAddress <= (nuint)address)
            {
                break;
            }

            address = (nint)endAddress;
        }

        return new MemoryScanSession(processId, valueType, pattern.Length, candidates);
    }

    public static MemoryScanSession NextScan(MemoryScanSession previousSession, ScanComparison comparison, string valueText, int maxResults = 10000)
    {
        byte[]? comparisonBytes = null;
        if (comparison == ScanComparison.EqualToValue)
        {
            comparisonBytes = GetValueBytes(previousSession.ValueType, valueText);
            if (comparisonBytes.Length != previousSession.ValueByteLength)
            {
                throw new InvalidOperationException("再次扫描字符串时，请输入与首次扫描相同字节长度的字符串。");
            }
        }

        using var processHandle = OpenReadableProcess(previousSession.ProcessId);
        var candidates = new List<MemoryScanCandidate>();

        foreach (var candidate in previousSession.Candidates)
        {
            if (candidates.Count >= maxResults)
            {
                break;
            }

            var currentBytes = new byte[previousSession.ValueByteLength];
            if (!ReadExact(processHandle, candidate.Address, currentBytes))
            {
                continue;
            }

            if (!MatchesComparison(previousSession.ValueType, candidate.PreviousBytes, currentBytes, comparison, comparisonBytes))
            {
                continue;
            }

            var currentValue = FormatValue(previousSession.ValueType, currentBytes);
            candidates.Add(new MemoryScanCandidate
            {
                Address = candidate.Address,
                PreviousBytes = currentBytes,
                PreviousValue = candidate.CurrentValue,
                CurrentValue = currentValue,
                RegionBaseAddress = candidate.RegionBaseAddress,
                RegionSize = candidate.RegionSize,
                Protect = candidate.Protect
            });
        }

        return new MemoryScanSession(previousSession.ProcessId, previousSession.ValueType, previousSession.ValueByteLength, candidates);
    }

    public static IReadOnlyList<MemoryScanResult> ToResults(MemoryScanSession session)
    {
        return session.Candidates
            .Select(candidate => new MemoryScanResult
            {
                Address = FormatAddress(candidate.Address),
                Value = candidate.CurrentValue,
                PreviousValue = candidate.PreviousValue,
                ValueType = FormatValueType(session.ValueType),
                RegionBaseAddress = candidate.RegionBaseAddress,
                RegionSize = candidate.RegionSize,
                Protect = candidate.Protect
            })
            .ToList();
    }

    public static IReadOnlyList<MemoryScanResult> ScanInt32(int processId, int value, int maxResults = 10000)
    {
        var session = FirstScan(processId, ScanValueType.Int32, value.ToString(), maxResults);
        return ToResults(session);
    }

    private static SafeProcessHandle OpenReadableProcess(int processId)
    {
        var processHandle = NativeMethods.OpenProcess(
            NativeMethods.ProcessQueryInformation | NativeMethods.ProcessVmRead,
            false,
            processId);

        if (processHandle.IsInvalid)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        return processHandle;
    }

    private static void ScanRegionForPattern(SafeProcessHandle processHandle, nuint baseAddress, nuint regionSize, uint protect, ScanValueType valueType, byte[] pattern, int maxResults, List<MemoryScanCandidate> candidates)
    {
        var readBuffer = new byte[ScanBufferSize];
        var carry = Array.Empty<byte>();
        var currentAddress = baseAddress;
        var endAddress = baseAddress + regionSize;

        while (currentAddress < endAddress && candidates.Count < maxResults)
        {
            var remaining = endAddress - currentAddress;
            var bytesToRead = remaining > ScanBufferSize ? (nuint)ScanBufferSize : remaining;

            if (!NativeMethods.ReadProcessMemory(processHandle, (nint)currentAddress, readBuffer, bytesToRead, out var bytesRead) || bytesRead == 0)
            {
                currentAddress += bytesToRead;
                carry = Array.Empty<byte>();
                continue;
            }

            var bytesReadAsInt = checked((int)bytesRead);
            var combined = new byte[carry.Length + bytesReadAsInt];
            Buffer.BlockCopy(carry, 0, combined, 0, carry.Length);
            Buffer.BlockCopy(readBuffer, 0, combined, carry.Length, bytesReadAsInt);

            for (var index = 0; index <= combined.Length - pattern.Length && candidates.Count < maxResults; index++)
            {
                if (index + pattern.Length <= carry.Length || !BytesEqual(combined, index, pattern))
                {
                    continue;
                }

                var matchAddress = currentAddress - (nuint)carry.Length + (nuint)index;
                candidates.Add(new MemoryScanCandidate
                {
                    Address = matchAddress,
                    PreviousBytes = pattern.ToArray(),
                    PreviousValue = string.Empty,
                    CurrentValue = FormatValue(valueType, pattern),
                    RegionBaseAddress = FormatAddress(baseAddress),
                    RegionSize = FormatSize(regionSize),
                    Protect = FormatProtect(protect)
                });
            }

            var carryLength = Math.Min(pattern.Length - 1, bytesReadAsInt);
            carry = new byte[carryLength];
            Buffer.BlockCopy(readBuffer, bytesReadAsInt - carryLength, carry, 0, carryLength);
            currentAddress += bytesRead;
        }
    }

    private static bool ReadExact(SafeProcessHandle processHandle, nuint address, byte[] buffer)
    {
        return NativeMethods.ReadProcessMemory(processHandle, (nint)address, buffer, (nuint)buffer.Length, out var bytesRead)
            && bytesRead == (nuint)buffer.Length;
    }

    private static bool MatchesComparison(ScanValueType valueType, byte[] previousBytes, byte[] currentBytes, ScanComparison comparison, byte[]? comparisonBytes)
    {
        return comparison switch
        {
            ScanComparison.EqualToValue => comparisonBytes is not null && BytesEqual(currentBytes, 0, comparisonBytes),
            ScanComparison.Unchanged => BytesEqual(currentBytes, 0, previousBytes),
            ScanComparison.Increased => CompareNumeric(valueType, currentBytes, previousBytes) > 0,
            ScanComparison.Decreased => CompareNumeric(valueType, currentBytes, previousBytes) < 0,
            _ => false
        };
    }

    private static int CompareNumeric(ScanValueType valueType, byte[] currentBytes, byte[] previousBytes)
    {
        if (valueType is ScanValueType.StringUtf8 or ScanValueType.StringUtf16)
        {
            throw new InvalidOperationException("字符串类型不支持变大/变小扫描，请使用“等于输入值”或“未变”。");
        }

        var current = ToDouble(valueType, currentBytes);
        var previous = ToDouble(valueType, previousBytes);
        return current.CompareTo(previous);
    }

    private static double ToDouble(ScanValueType valueType, byte[] bytes)
    {
        return valueType switch
        {
            ScanValueType.Int32 => BitConverter.ToInt32(bytes),
            ScanValueType.Float => BitConverter.ToSingle(bytes),
            ScanValueType.Double => BitConverter.ToDouble(bytes),
            _ => throw new InvalidOperationException("当前类型不能转换为数值。")
        };
    }

    private static bool BytesEqual(byte[] source, int sourceIndex, byte[] pattern)
    {
        for (var index = 0; index < pattern.Length; index++)
        {
            if (source[sourceIndex + index] != pattern[index])
            {
                return false;
            }
        }

        return true;
    }

    private static byte[] GetValueBytes(ScanValueType valueType, string valueText)
    {
        return valueType switch
        {
            ScanValueType.Int32 => int.TryParse(valueText, out var intValue)
                ? BitConverter.GetBytes(intValue)
                : throw new InvalidOperationException("请输入有效的 32 位整数。"),
            ScanValueType.Float => float.TryParse(valueText, out var floatValue)
                ? BitConverter.GetBytes(floatValue)
                : throw new InvalidOperationException("请输入有效的 Float 数值。"),
            ScanValueType.Double => double.TryParse(valueText, out var doubleValue)
                ? BitConverter.GetBytes(doubleValue)
                : throw new InvalidOperationException("请输入有效的 Double 数值。"),
            ScanValueType.StringUtf8 => string.IsNullOrEmpty(valueText)
                ? throw new InvalidOperationException("字符串不能为空。")
                : Encoding.UTF8.GetBytes(valueText),
            ScanValueType.StringUtf16 => string.IsNullOrEmpty(valueText)
                ? throw new InvalidOperationException("字符串不能为空。")
                : Encoding.Unicode.GetBytes(valueText),
            _ => throw new InvalidOperationException("未知扫描类型。")
        };
    }

    private static string FormatValue(ScanValueType valueType, byte[] bytes)
    {
        return valueType switch
        {
            ScanValueType.Int32 => BitConverter.ToInt32(bytes).ToString(),
            ScanValueType.Float => BitConverter.ToSingle(bytes).ToString("G9"),
            ScanValueType.Double => BitConverter.ToDouble(bytes).ToString("G17"),
            ScanValueType.StringUtf8 => Encoding.UTF8.GetString(bytes),
            ScanValueType.StringUtf16 => Encoding.Unicode.GetString(bytes),
            _ => string.Empty
        };
    }

    private static string FormatValueType(ScanValueType valueType)
    {
        return valueType switch
        {
            ScanValueType.Int32 => "Int32",
            ScanValueType.Float => "Float",
            ScanValueType.Double => "Double",
            ScanValueType.StringUtf8 => "String UTF-8",
            ScanValueType.StringUtf16 => "String UTF-16",
            _ => valueType.ToString()
        };
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

    private static string DescribeState(uint state)
    {
        return state switch
        {
            NativeMethods.MemCommit => "已提交：这段虚拟地址已有实际存储支持，可读写取决于 Protect。",
            NativeMethods.MemReserve => "已保留：地址范围被占用，但还未分配实际存储，通常不能直接读取。",
            NativeMethods.MemFree => "空闲：这段地址未被当前进程使用。",
            _ => "未知状态。"
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

    private static string DescribeType(uint type)
    {
        return type switch
        {
            0 => string.Empty,
            NativeMethods.MemPrivate => "私有内存：通常是堆、栈或进程自己分配的数据。",
            NativeMethods.MemMapped => "映射内存：来自文件映射或共享内存。",
            NativeMethods.MemImage => "映像内存：来自 EXE/DLL 模块映射。",
            _ => "未知类型。"
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

    private static string DescribeProtect(uint protect)
    {
        if (protect == 0)
        {
            return string.Empty;
        }

        if ((protect & NativeMethods.PageGuard) != 0)
        {
            return "保护页：访问时可能触发异常，扫描时通常跳过。";
        }

        if ((protect & NativeMethods.PageNoAccess) != 0)
        {
            return "不可访问：不能读取或写入。";
        }

        var baseProtect = protect & 0xff;
        return baseProtect switch
        {
            NativeMethods.PageReadonly => "只读：可以读取，不能写入。",
            NativeMethods.PageReadwrite => "读写：可以读取和写入，常见于变量数据。",
            NativeMethods.PageWritecopy => "写入时复制：可读，写入时会创建私有副本。",
            NativeMethods.PageExecute => "仅执行：通常是代码页，不适合作为数值扫描目标。",
            NativeMethods.PageExecuteRead => "执行/读取：常见于代码或只读模块数据。",
            NativeMethods.PageExecuteReadwrite => "执行/读写：可读写且可执行。",
            NativeMethods.PageExecuteWritecopy => "执行/写入时复制：可读，写入时复制。",
            _ => "未知保护属性。"
        };
    }
}
