using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static TwitchLeecher.Common.Win32;

namespace TwitchLeecher.Common
{
    public class Display
    {
        #region Fields

        private bool isPrimary;
        private string name;
        private IntPtr handle;
        private System.Windows.Rect bounds;
        private System.Windows.Rect workingArea;

        #endregion Fields

        #region Constructor

        private Display(IntPtr hMonitor, IntPtr hdc)
        {
            MonitorInfoEx info = new MonitorInfoEx();
            GetMonitorInfo(new HandleRef(null, hMonitor), info);

            this.isPrimary = ((info.dwFlags & MonitorinfofPrimary) != 0);
            this.name = new string(info.szDevice).TrimEnd((char)0);
            this.handle = hMonitor;

            this.bounds = new System.Windows.Rect(
                        info.rcMonitor.left, info.rcMonitor.top,
                        info.rcMonitor.right - info.rcMonitor.left,
                        info.rcMonitor.bottom - info.rcMonitor.top);

            this.workingArea = new System.Windows.Rect(
                        info.rcWork.left, info.rcWork.top,
                        info.rcWork.right - info.rcWork.left,
                        info.rcWork.bottom - info.rcWork.top);
        }

        #endregion Constructor

        #region Properties

        public bool IsPrimary
        {
            get

            {
                return this.isPrimary;
            }
        }

        public string Name

        {
            get
            {
                return this.name;
            }
        }

        public IntPtr Handle
        {
            get
            {
                return this.handle;
            }
        }

        public System.Windows.Rect Bounds
        {
            get
            {
                return this.bounds;
            }
        }

        public System.Windows.Rect WorkingArea
        {
            get
            {
                return this.workingArea;
            }
        }

        #endregion Properties

        #region Methods

        public static IEnumerable<Display> GetAllDisplays()
        {
            DisplayEnumCallback closure = new DisplayEnumCallback();
            MonitorEnumProc proc = new MonitorEnumProc(closure.Callback);
            EnumDisplayMonitors(new HandleRef(null, IntPtr.Zero), IntPtr.Zero, proc, IntPtr.Zero);
            return closure.Displays.Cast<Display>();
        }

        public static Display GetDisplayFromWindow(IntPtr handle)
        {
            IntPtr hMonitor = MonitorFromWindow(handle, MONITOR_DEFAULTTONEAREST);

            return GetAllDisplays().Where(d => d.Handle == hMonitor).First();
        }

        #endregion Methods

        #region Classes

        private class DisplayEnumCallback
        {
            public ArrayList Displays { get; private set; }

            public DisplayEnumCallback()
            {
                this.Displays = new ArrayList();
            }

            public bool Callback(IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData)
            {
                this.Displays.Add(new Display(hMonitor, hdcMonitor));
                return true;
            }
        }

        #endregion Classes
    }
}