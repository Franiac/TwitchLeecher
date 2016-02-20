using FontAwesome.WPF;
using Prism.Events;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using TwitchLeecher.Common;
using TwitchLeecher.Core.Events;
using static TwitchLeecher.Common.Win32;

namespace TwitchLeecher
{
    public partial class Shell : Window
    {
        #region Fields

        private IEventAggregator eventAggregator;

        #endregion Fields

        #region Constructors

        public Shell(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            // Keep reference to FontAwesome.WPF.dll
            ImageAwesome.CreateImageSource(FontAwesomeIcon.Twitch, Brushes.Black);

            WindowChrome windowChrome = new WindowChrome()
            {
                CaptionHeight = 55,
                CornerRadius = new CornerRadius(0),
                GlassFrameThickness = new Thickness(0),
                NonClientFrameEdges = NonClientFrameEdges.None,
                ResizeBorderThickness = new Thickness(6),
                UseAeroCaptionButtons = false
            };

            WindowChrome.SetWindowChrome(this, windowChrome);

            this.eventAggregator = eventAggregator;

            AssemblyUtil au = AssemblyUtil.Get;

            this.Title = au.GetProductName() + " " + au.GetAssemblyVersion().Trim();

            this.eventAggregator.GetEvent<AppMinimizeEvent>().Subscribe(() =>
            {
                this.WindowState = WindowState.Minimized;
            });

            this.eventAggregator.GetEvent<AppMaximizeRestoreEvent>().Subscribe(() =>
            {
                this.WindowState = this.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
            });

            this.eventAggregator.GetEvent<AppExitEvent>().Subscribe(() =>
            {
                this.Close();
            });

            this.Loaded += (s, e) =>
            {
                HwndSource.FromHwnd(new WindowInteropHelper(this).Handle).AddHook(new HwndSourceHook(WindowProc));
            };
        }

        #endregion Constructors

        #region Methods

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handeled)
        {
            switch (msg)
            {
                case WM_GETMINMAXINFO:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handeled = true;
                    break;

                case WM_WINDOWPOSCHANGING:
                    WINDOWPOS pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

                    if ((pos.flags & 0x0002) != 0)
                    {
                        return IntPtr.Zero;
                    }

                    Window wnd = (Window)HwndSource.FromHwnd(hwnd).RootVisual;
                    if (wnd == null)
                    {
                        return IntPtr.Zero;
                    }

                    bool changedPos = false;

                    PresentationSource source = PresentationSource.FromVisual(this);

                    double wpfDpi = 96;

                    double dpiX = wpfDpi;
                    double dpiY = wpfDpi;

                    if (source != null)
                    {
                        dpiX = wpfDpi * source.CompositionTarget.TransformToDevice.M11;
                        dpiY = wpfDpi * source.CompositionTarget.TransformToDevice.M22;
                    }

                    int minWidth = (int)Math.Round(this.MinWidth / 96 * dpiX, 0);

                    if (pos.cx < minWidth)
                    {
                        pos.cx = minWidth;
                        changedPos = true;
                    }

                    int minHeight = (int)Math.Round(this.MinHeight / 96 * dpiY, 0);

                    if (pos.cy < minHeight)
                    {
                        pos.cy = minHeight;
                        changedPos = true;
                    }

                    if (!changedPos)
                    {
                        return IntPtr.Zero;
                    }

                    Marshal.StructureToPtr(pos, lParam, true);
                    handeled = true;
                    break;
            }

            return (System.IntPtr)0;
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            IntPtr hMonitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            MonitorInfoEx info = new MonitorInfoEx();
            GetMonitorInfo(new HandleRef(null, hMonitor), info);

            Win32.Rect rcWorkArea = info.rcWork;
            Win32.Rect rcMonitorArea = info.rcMonitor;

            mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
            mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
            mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
            mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        #endregion Methods

        #region EventHandler

        private void Window_StateChanged(object sender, EventArgs e)
        {
            this.eventAggregator.GetEvent<AppMaximizedChangedEvent>().Publish(this.WindowState == WindowState.Maximized);
        }

        #endregion EventHandler
    }
}