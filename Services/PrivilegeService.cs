using System.ComponentModel;
using System.Runtime.InteropServices;
using LCE.Native;

namespace LCE.Services;

public static class PrivilegeService
{
    private const int ErrorNotAllAssigned = 1300;
    private const string SeDebugPrivilege = "SeDebugPrivilege";

    public static string LastDebugPrivilegeStatus { get; private set; } = "未尝试启用调试权限。";

    public static bool TryEnableDebugPrivilege()
    {
        if (!NativeMethods.OpenProcessToken(
            NativeMethods.GetCurrentProcess(),
            NativeMethods.TokenAdjustPrivileges | NativeMethods.TokenQuery,
            out var tokenHandle))
        {
            LastDebugPrivilegeStatus = $"打开进程令牌失败：{new Win32Exception(Marshal.GetLastWin32Error()).Message}";
            return false;
        }

        using (tokenHandle)
        {
            if (!NativeMethods.LookupPrivilegeValue(null, SeDebugPrivilege, out var luid))
            {
                LastDebugPrivilegeStatus = $"查找 {SeDebugPrivilege} 失败：{new Win32Exception(Marshal.GetLastWin32Error()).Message}";
                return false;
            }

            var privileges = new NativeMethods.TokenPrivileges
            {
                PrivilegeCount = 1,
                Privileges = new NativeMethods.LuidAndAttributes
                {
                    Luid = luid,
                    Attributes = NativeMethods.SePrivilegeEnabled
                }
            };

            if (!NativeMethods.AdjustTokenPrivileges(tokenHandle, false, ref privileges, 0, 0, 0))
            {
                LastDebugPrivilegeStatus = $"启用 {SeDebugPrivilege} 失败：{new Win32Exception(Marshal.GetLastWin32Error()).Message}";
                return false;
            }

            var errorCode = Marshal.GetLastWin32Error();
            if (errorCode == ErrorNotAllAssigned)
            {
                LastDebugPrivilegeStatus = $"当前账户未被授予 {SeDebugPrivilege}。请以管理员身份运行，或检查本地安全策略。";
                return false;
            }

            LastDebugPrivilegeStatus = $"已启用 {SeDebugPrivilege}。";
            return true;
        }
    }
}
