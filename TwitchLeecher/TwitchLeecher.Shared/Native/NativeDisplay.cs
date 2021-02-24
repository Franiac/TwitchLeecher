using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static TwitchLeecher.Shared.Native.NativeDelegates;
using static TwitchLeecher.Shared.Native.NativeMethods;
using static TwitchLeecher.Shared.Native.NativeStructs;

namespace TwitchLeecher.Shared.Native
{
    public class NativeDisplay
    {
        #region Constructor

        private NativeDisplay(IntPtr hMonitor)
        {
            MonitorInfoEx info = new MonitorInfoEx();
            GetMonitorInfoNative(new HandleRef(null, hMonitor), info);

            IsPrimary = ((info.dwFlags & NativeFlags.MonitorinfofPrimary) != 0);
            Name = new string(info.szDevice).TrimEnd((char)0);
            Handle = hMonitor;

            Bounds = new System.Windows.Rect(
                        info.rcMonitor.left, info.rcMonitor.top,
                        info.rcMonitor.right - info.rcMonitor.left,
                        info.rcMonitor.bottom - info.rcMonitor.top);

            WorkingArea = new System.Windows.Rect(
                        info.rcWork.left, info.rcWork.top,
                        info.rcWork.right - info.rcWork.left,
                        info.rcWork.bottom - info.rcWork.top);
        }

        #endregion Constructor

        #region Properties

        public System.Windows.Rect Bounds { get; }

        public IntPtr Handle { get; }

        public bool IsPrimary { get; }

        public string Name { get; }

        public System.Windows.Rect WorkingArea { get; }

        #endregion Properties

        #region Methods

        public static IEnumerable<NativeDisplay> GetAllDisplays()
        {
            ArrayList displays = new ArrayList();

            MonitorEnumProc proc = new MonitorEnumProc((IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData) =>
            {
                displays.Add(new NativeDisplay(hMonitor));
                return true;
            });

            EnumDisplayMonitorsNative(new HandleRef(null, IntPtr.Zero), IntPtr.Zero, proc, IntPtr.Zero);

            return displays.Cast<NativeDisplay>();
        }

        public static NativeDisplay GetDisplayFromWindow(IntPtr handle)
        {
            IntPtr hMonitor = MonitorFromWindowNative(handle, NativeFlags.MONITOR_DEFAULTTONEAREST);

            return GetAllDisplays().Where(d => d.Handle == hMonitor).First();
        }

        #endregion Methods
    }
}