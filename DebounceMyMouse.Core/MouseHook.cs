using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DebounceMyMouse.Core;
public class MouseHook
{
    public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
    private const int WH_MOUSE_LL = 14;
    private static IntPtr _hookID = IntPtr.Zero;
    private static HookProc _proc;

    public static event Func<string, bool>? ShouldBlock;

    public static void Start()
    {
        _proc = HookCallback;
        _hookID = SetHook(_proc);
    }

    public static void Stop()
    {
        UnhookWindowsHookEx(_hookID);
    }

    private static IntPtr SetHook(HookProc proc)
    {
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule!;
        return SetWindowsHookEx(WH_MOUSE_LL, proc,
            GetModuleHandle(curModule.ModuleName), 0);
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int msg = wParam.ToInt32();
            string? input = msg switch
            {
                0x0201 => "Left",
                0x0204 => "Right",
                0x0207 => "Middle",
                _ => null
            };
            if (input is not null)
            {
                // Check if the input should be blocked
                if (ShouldBlock?.Invoke(input) == true)
                {
                    return (IntPtr)1; // Block the event
                }
            }
        }

        return CallNextHookEx(_hookID, nCode, wParam, lParam); // Pass event along
    }

    #region WinAPI

    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn,
        IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll")]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
        IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    #endregion
}
