using System.Runtime.InteropServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace LCE.Native;

internal static class NativeMethods
{
    internal const uint ProcessQueryInformation = 0x0400;
    internal const uint ProcessVmRead = 0x0010;

    internal const uint MemCommit = 0x1000;
    internal const uint MemReserve = 0x2000;
    internal const uint MemFree = 0x10000;

    internal const uint MemPrivate = 0x20000;
    internal const uint MemMapped = 0x40000;
    internal const uint MemImage = 0x1000000;

    internal const uint PageNoAccess = 0x01;
    internal const uint PageReadonly = 0x02;
    internal const uint PageReadwrite = 0x04;
    internal const uint PageWritecopy = 0x08;
    internal const uint PageExecute = 0x10;
    internal const uint PageExecuteRead = 0x20;
    internal const uint PageExecuteReadwrite = 0x40;
    internal const uint PageExecuteWritecopy = 0x80;
    internal const uint PageGuard = 0x100;
    internal const uint PageNocache = 0x200;
    internal const uint PageWriteCombine = 0x400;

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern SafeProcessHandle OpenProcess(uint desiredAccess, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, int processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern nuint VirtualQueryEx(SafeProcessHandle processHandle, nint address, out MemoryBasicInformation buffer, nuint length);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool ReadProcessMemory(SafeProcessHandle processHandle, nint baseAddress, byte[] buffer, nuint size, out nuint bytesRead);

    [DllImport("kernel32.dll")]
    internal static extern void GetNativeSystemInfo(out SystemInfo systemInfo);

    [StructLayout(LayoutKind.Sequential)]
    internal struct MemoryBasicInformation
    {
        public nint BaseAddress;
        public nint AllocationBase;
        public uint AllocationProtect;
        public nuint RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SystemInfo
    {
        public ushort ProcessorArchitecture;
        public ushort Reserved;
        public uint PageSize;
        public nint MinimumApplicationAddress;
        public nint MaximumApplicationAddress;
        public nuint ActiveProcessorMask;
        public uint NumberOfProcessors;
        public uint ProcessorType;
        public uint AllocationGranularity;
        public ushort ProcessorLevel;
        public ushort ProcessorRevision;
    }
}
