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
        #region Fields

        private System.Windows.Rect _bounds;
        private IntPtr _handle;
        private bool _isPrimary;
        private string _name;
        private System.Windows.Rect _workingArea;

        #endregion Fields

        #region Constructor

        private NativeDisplay(IntPtr hMonitor, IntPtr hdc)
        {
            MonitorInfoEx info = new MonitorInfoEx();
            GetMonitorInfoNative(new HandleRef(null, hMonitor), info);

            _isPrimary = ((info.dwFlags & NativeFlags.MonitorinfofPrimary) != 0);
            _name = new string(info.szDevice).TrimEnd((char)0);
            _handle = hMonitor;

            _bounds = new System.Windows.Rect(
                        info.rcMonitor.left, info.rcMonitor.top,
                        info.rcMonitor.right - info.rcMonitor.left,
                        info.rcMonitor.bottom - info.rcMonitor.top);

            _workingArea = new System.Windows.Rect(
                        info.rcWork.left, info.rcWork.top,
                        info.rcWork.right - info.rcWork.left,
                        info.rcWork.bottom - info.rcWork.top);
        }

        #endregion Constructor

        #region Properties

        public System.Windows.Rect Bounds
        {
            get
            {
                return _bounds;
            }
        }

        public IntPtr Handle
        {
            get
            {
                return _handle;
            }
        }

        public bool IsPrimary
        {
            get

            {
                return _isPrimary;
            }
        }

        public string Name

        {
            get
            {
                return _name;
            }
        }

        public System.Windows.Rect WorkingArea
        {
            get
            {
                return _workingArea;
            }
        }

        #endregion Properties

        #region Methods

        public static IEnumerable<NativeDisplay> GetAllDisplays()
        {
            DisplayEnumCallback closure = new DisplayEnumCallback();
            MonitorEnumProc proc = new MonitorEnumProc(closure.Callback);
            EnumDisplayMonitorsNative(new HandleRef(null, IntPtr.Zero), IntPtr.Zero, proc, IntPtr.Zero);
            return closure.Displays.Cast<NativeDisplay>();
        }

        public static NativeDisplay GetDisplayFromWindow(IntPtr handle)
        {
            IntPtr hMonitor = MonitorFromWindowNative(handle, NativeFlags.MONITOR_DEFAULTTONEAREST);

            return GetAllDisplays().Where(d => d.Handle == hMonitor).First();
        }

        #endregion Methods

        #region Classes

        private class DisplayEnumCallback
        {
            #region Constructors

            public DisplayEnumCallback()
            {
                Displays = new ArrayList();
            }

            #endregion Constructors

            #region Properties

            public ArrayList Displays { get; private set; }

            #endregion Properties

            #region Methods

            public bool Callback(IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData)
            {
                Displays.Add(new NativeDisplay(hMonitor, hdcMonitor));
                return true;
            }

            #endregion Methods
        }

        #endregion Classes
    }
}