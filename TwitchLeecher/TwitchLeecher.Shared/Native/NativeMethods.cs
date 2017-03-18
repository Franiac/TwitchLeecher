using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using static TwitchLeecher.Shared.Native.NativeDelegates;
using static TwitchLeecher.Shared.Native.NativeStructs;

namespace TwitchLeecher.Shared.Native
{
    public static class NativeMethods
    {
        [DllImport("user32.dll", ExactSpelling = true)]
        [ResourceExposure(ResourceScope.None)]
        private static extern bool EnumDisplayMonitors(HandleRef hdc, IntPtr rcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

        public static bool EnumDisplayMonitorsNative(HandleRef hdc, IntPtr rcClip, MonitorEnumProc lpfnEnum, IntPtr dwData)
        {
            return EnumDisplayMonitors(hdc, rcClip, lpfnEnum, dwData);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        private static extern bool GetMonitorInfo(HandleRef hMonitor, [In, Out]MonitorInfoEx lpmi);

        public static bool GetMonitorInfoNative(HandleRef hMonitor, [In, Out]MonitorInfoEx lpmi)
        {
            return GetMonitorInfo(hMonitor, lpmi);
        }

        [DllImport("user32.dll")]
        [ResourceExposure(ResourceScope.None)]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        public static IntPtr MonitorFromWindowNative(IntPtr hwnd, uint dwFlags)
        {
            return MonitorFromWindow(hwnd, dwFlags);
        }
    }
}