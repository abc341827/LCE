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

    public static void CleanupTemporaryScanFiles()
    {
        var directory = GetScanDirectory();
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var filePath in Directory.EnumerateFiles(directory, "scan_*.bin"))
        {
            TryDeleteFile(filePath);
        }
    }

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

    public static MemoryScanSession FirstScan(int processId, ScanValueType valueType, string valueText, bool privateReadWriteOnly)
    {
        var pattern = GetValueBytes(valueType, valueText);

        using var processHandle = OpenReadableProcess(processId);
        NativeMethods.GetNativeSystemInfo(out var systemInfo);

        var resultFilePath = CreateScanFilePath();
        using var resultStream = File.Create(resultFilePath);
        using var writer = new BinaryWriter(resultStream);
        long resultCount = 0;
        var address = systemInfo.MinimumApplicationAddress;
        var maximumAddress = systemInfo.MaximumApplicationAddress;
        var informationSize = (nuint)Marshal.SizeOf<NativeMethods.MemoryBasicInformation>();

        while ((nuint)address < (nuint)maximumAddress)
        {
            var queryResult = NativeMethods.VirtualQueryEx(processHandle, address, out var information, informationSize);
            if (queryResult == 0)
            {
                break;
            }

            var baseAddress = (nuint)information.BaseAddress;
            var regionSize = information.RegionSize;
            var endAddress = baseAddress + regionSize;

            if (IsScannableRegion(information.State, information.Protect, information.Type, privateReadWriteOnly))
            {
                resultCount += ScanRegionForPattern(processHandle, baseAddress, regionSize, pattern, writer);
            }

            if (endAddress <= (nuint)address)
            {
                break;
            }

            address = (nint)endAddress;
        }

        return new MemoryScanSession(processId, valueType, pattern.Length, resultFilePath, resultCount);
    }

    public static MemoryScanSession FirstUnknownScan(int processId, ScanValueType valueType, bool privateReadWriteOnly)
    {
        var valueByteLength = GetValueByteLength(valueType);

        using var processHandle = OpenReadableProcess(processId);
        NativeMethods.GetNativeSystemInfo(out var systemInfo);

        var resultFilePath = CreateScanFilePath();
        using var resultStream = File.Create(resultFilePath);
        using var writer = new BinaryWriter(resultStream);
        long resultCount = 0;
        var address = systemInfo.MinimumApplicationAddress;
        var maximumAddress = systemInfo.MaximumApplicationAddress;
        var informationSize = (nuint)Marshal.SizeOf<NativeMethods.MemoryBasicInformation>();

        while ((nuint)address < (nuint)maximumAddress)
        {
            var queryResult = NativeMethods.VirtualQueryEx(processHandle, address, out var information, informationSize);
            if (queryResult == 0)
            {
                break;
            }

            var baseAddress = (nuint)information.BaseAddress;
            var regionSize = information.RegionSize;
            var endAddress = baseAddress + regionSize;

            if (IsScannableRegion(information.State, information.Protect, information.Type, privateReadWriteOnly))
            {
                resultCount += ScanRegionForUnknownValues(processHandle, baseAddress, regionSize, valueType, valueByteLength, writer);
            }

            if (endAddress <= (nuint)address)
            {
                break;
            }

            address = (nint)endAddress;
        }

        return new MemoryScanSession(processId, valueType, valueByteLength, resultFilePath, resultCount);
    }

    public static MemoryScanSession NextScan(MemoryScanSession previousSession, ScanComparison comparison, string valueText)
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
        var resultFilePath = CreateScanFilePath();
        long resultCount = 0;

        using (var inputStream = File.OpenRead(previousSession.ResultFilePath))
        using (var reader = new BinaryReader(inputStream))
        using (var outputStream = File.Create(resultFilePath))
        using (var writer = new BinaryWriter(outputStream))
        {
            while (TryReadRecord(reader, previousSession.ValueByteLength, out var candidateAddress, out var previousBytes))
            {
                var currentBytes = new byte[previousSession.ValueByteLength];
                if (!ReadExact(processHandle, candidateAddress, currentBytes))
                {
                    continue;
                }

                if (!MatchesComparison(previousSession.ValueType, previousBytes, currentBytes, comparison, comparisonBytes))
                {
                    continue;
                }

                WriteRecord(writer, candidateAddress, currentBytes);
                resultCount++;
            }
        }

        TryDeleteFile(previousSession.ResultFilePath);
        return new MemoryScanSession(previousSession.ProcessId, previousSession.ValueType, previousSession.ValueByteLength, resultFilePath, resultCount);
    }

    public static IReadOnlyList<MemoryScanResult> ToResults(MemoryScanSession session, int maxPreviewResults = 10000)
    {
        var results = new List<MemoryScanResult>();
        using var inputStream = File.OpenRead(session.ResultFilePath);
        using var reader = new BinaryReader(inputStream);

        while (results.Count < maxPreviewResults && TryReadRecord(reader, session.ValueByteLength, out var address, out var valueBytes))
        {
            results.Add(new MemoryScanResult
            {
                Address = FormatAddress(address),
                Value = FormatValue(session.ValueType, valueBytes),
                ValueType = FormatValueType(session.ValueType)
            });
        }

        return results;
    }

    public static IReadOnlyList<MemoryScanResult> ScanInt32(int processId, int value)
    {
        var session = FirstScan(processId, ScanValueType.Int32, value.ToString(), false);
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

    private static string CreateScanFilePath()
    {
        var directory = GetScanDirectory();
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, $"scan_{Guid.NewGuid():N}.bin");
    }

    private static string GetScanDirectory()
    {
        return Path.Combine(Path.GetTempPath(), "LCE", "Scans");
    }

    private static void WriteRecord(BinaryWriter writer, nuint address, byte[] valueBytes)
    {
        writer.Write((ulong)address);
        writer.Write(valueBytes);
    }

    private static bool TryReadRecord(BinaryReader reader, int valueByteLength, out nuint address, out byte[] valueBytes)
    {
        address = 0;
        valueBytes = Array.Empty<byte>();

        var recordLength = sizeof(ulong) + valueByteLength;
        if (reader.BaseStream.Length - reader.BaseStream.Position < recordLength)
        {
            return false;
        }

        address = (nuint)reader.ReadUInt64();
        valueBytes = reader.ReadBytes(valueByteLength);
        return valueBytes.Length == valueByteLength;
    }

    private static void TryDeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
        }
    }

    private static long ScanRegionForPattern(SafeProcessHandle processHandle, nuint baseAddress, nuint regionSize, byte[] pattern, BinaryWriter writer)
    {
        var readBuffer = new byte[ScanBufferSize];
        var carry = Array.Empty<byte>();
        var currentAddress = baseAddress;
        var endAddress = baseAddress + regionSize;
        long resultCount = 0;

        while (currentAddress < endAddress)
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

            for (var index = 0; index <= combined.Length - pattern.Length; index++)
            {
                if (index + pattern.Length <= carry.Length || !BytesEqual(combined, index, pattern))
                {
                    continue;
                }

                var matchAddress = currentAddress - (nuint)carry.Length + (nuint)index;
                WriteRecord(writer, matchAddress, pattern);
                resultCount++;
            }

            var carryLength = Math.Min(pattern.Length - 1, bytesReadAsInt);
            carry = new byte[carryLength];
            Buffer.BlockCopy(readBuffer, bytesReadAsInt - carryLength, carry, 0, carryLength);
            currentAddress += bytesRead;
        }

        return resultCount;
    }

    private static long ScanRegionForUnknownValues(SafeProcessHandle processHandle, nuint baseAddress, nuint regionSize, ScanValueType valueType, int valueByteLength, BinaryWriter writer)
    {
        var readBuffer = new byte[ScanBufferSize];
        var currentAddress = baseAddress;
        var endAddress = baseAddress + regionSize;
        long resultCount = 0;

        while (currentAddress < endAddress)
        {
            var remaining = endAddress - currentAddress;
            var bytesToRead = remaining > ScanBufferSize ? (nuint)ScanBufferSize : remaining;

            if (!NativeMethods.ReadProcessMemory(processHandle, (nint)currentAddress, readBuffer, bytesToRead, out var bytesRead) || bytesRead < (nuint)valueByteLength)
            {
                currentAddress += bytesToRead;
                continue;
            }

            var bytesReadAsInt = checked((int)bytesRead);
            for (var index = 0; index <= bytesReadAsInt - valueByteLength; index += valueByteLength)
            {
                var valueBytes = new byte[valueByteLength];
                Buffer.BlockCopy(readBuffer, index, valueBytes, 0, valueByteLength);
                if (!IsUsefulUnknownValue(valueType, valueBytes))
                {
                    continue;
                }

                WriteRecord(writer, currentAddress + (nuint)index, valueBytes);
                resultCount++;
            }

            currentAddress += bytesRead;
        }

        return resultCount;
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
            ScanComparison.Changed => !BytesEqual(currentBytes, 0, previousBytes),
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

    private static int GetValueByteLength(ScanValueType valueType)
    {
        return valueType switch
        {
            ScanValueType.Int32 => sizeof(int),
            ScanValueType.Float => sizeof(float),
            ScanValueType.Double => sizeof(double),
            ScanValueType.StringUtf8 or ScanValueType.StringUtf16 => throw new InvalidOperationException("字符串不支持未知初始值扫描，请输入明确字符串后首次扫描。"),
            _ => throw new InvalidOperationException("未知扫描类型。")
        };
    }

    private static bool IsUsefulUnknownValue(ScanValueType valueType, byte[] bytes)
    {
        return valueType switch
        {
            ScanValueType.Float => !float.IsNaN(BitConverter.ToSingle(bytes)) && !float.IsInfinity(BitConverter.ToSingle(bytes)),
            ScanValueType.Double => !double.IsNaN(BitConverter.ToDouble(bytes)) && !double.IsInfinity(BitConverter.ToDouble(bytes)),
            _ => true
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

    private static bool IsScannableRegion(uint state, uint protect, uint type, bool privateReadWriteOnly)
    {
        if (!IsReadable(state, protect))
        {
            return false;
        }

        if (!privateReadWriteOnly)
        {
            return true;
        }

        var baseProtect = protect & 0xff;
        return type == NativeMethods.MemPrivate && baseProtect == NativeMethods.PageReadwrite;
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
