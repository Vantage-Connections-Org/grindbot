using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace GrindBot;

/// The Win32 bits AppKit gave us for free: a click-through, never-focused,
/// always-on-top overlay, and honest physical-pixel placement on a chosen
/// monitor regardless of per-monitor DPI.
internal static class Native
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;   // clicks pass straight through
    private const int WS_EX_TOOLWINDOW  = 0x00000080;   // no taskbar / alt-tab entry
    private const int WS_EX_NOACTIVATE  = 0x08000000;   // never takes focus
    private const int WS_EX_LAYERED     = 0x00080000;

    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;
    private static readonly IntPtr HWND_TOPMOST = new(-1);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    /// Click-through + focus-proof. Called once the HWND exists.
    public static void MakeOverlay(Window w)
    {
        var hwnd = new WindowInteropHelper(w).Handle;
        if (hwnd == IntPtr.Zero) return;
        int ex = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE,
            ex | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE);
    }

    /// Move in physical pixels, so mixed-DPI multi-monitor setups land right.
    public static void MoveTo(Window w, int x, int y)
    {
        var hwnd = new WindowInteropHelper(w).Handle;
        if (hwnd == IntPtr.Zero) return;
        SetWindowPos(hwnd, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
    }

    public static (int W, int H) PhysicalSize(Window w)
    {
        var hwnd = new WindowInteropHelper(w).Handle;
        if (hwnd == IntPtr.Zero || !GetWindowRect(hwnd, out var r)) return (0, 0);
        return (r.Right - r.Left, r.Bottom - r.Top);
    }

    /// Re-assert top-of-stack without stealing focus — the WPF Topmost flag
    /// alone loses ground to things like the shell's own always-on-top windows.
    public static void BumpTopmost(Window w)
    {
        var hwnd = new WindowInteropHelper(w).Handle;
        if (hwnd == IntPtr.Zero) return;
        SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
    }
}

/// Run at login. Windows has no login-items UI equivalent to macOS's, so the
/// tray menu owns it: one HKCU Run value, removable from the same menu.
internal static class RunAtLogin
{
    private const string Key = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string Name = "GrindBot";

    public static bool Enabled
    {
        get
        {
            try
            {
                using var k = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(Key);
                return k?.GetValue(Name) is string v && v.Contains("GrindBot.exe", StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }
    }

    /// Returns an error string on failure; a locked-down registry shouldn't crash us.
    public static string? Set(bool on)
    {
        try
        {
            using var k = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(Key);
            if (k is null) return "Couldn't open the Run key.";
            if (on) k.SetValue(Name, "\"" + (Environment.ProcessPath ?? "") + "\"");
            else k.DeleteValue(Name, false);
            return null;
        }
        catch (Exception e) { return e.Message; }
    }
}

internal static class Theme
{
    /// Windows' own light/dark setting, so the bubble reads like system chrome
    /// the way macOS's .regularMaterial did.
    public static bool IsDark(string themeSetting)
    {
        if (string.Equals(themeSetting, "dark", StringComparison.OrdinalIgnoreCase)) return true;
        if (string.Equals(themeSetting, "light", StringComparison.OrdinalIgnoreCase)) return false;
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key?.GetValue("AppsUseLightTheme") is int v) return v == 0;
        }
        catch { /* registry locked down: fall through to dark */ }
        return true;
    }

    // No backdrop blur on a click-through layered window, so the bubble is a
    // translucent plate instead — same read, no compositor games.
    public static Color BubbleFill(bool dark) =>
        dark ? Color.FromArgb(0xE8, 0x22, 0x22, 0x26) : Color.FromArgb(0xEE, 0xF6, 0xF6, 0xF8);

    public static Color BubbleStroke(bool dark) =>
        dark ? Color.FromArgb(0x38, 0xFF, 0xFF, 0xFF) : Color.FromArgb(0x22, 0x00, 0x00, 0x00);

    public static Color BubbleText(bool dark) =>
        dark ? Color.FromRgb(0xF2, 0xF2, 0xF4) : Color.FromRgb(0x1A, 0x1A, 0x1C);
}
