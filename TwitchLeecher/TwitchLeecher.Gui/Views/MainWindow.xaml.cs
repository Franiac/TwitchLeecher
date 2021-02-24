using FontAwesome.WPF;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Gui.ViewModels;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Native;
using static TwitchLeecher.Shared.Native.NativeMethods;
using static TwitchLeecher.Shared.Native.NativeStructs;

namespace TwitchLeecher.Gui.Views
{
    public partial class MainWindow : Window
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly IRuntimeDataService _runtimeDataService;

        #endregion Fields

        #region Constructors

        public MainWindow(
            MainWindowVM viewModel,
            IDialogService dialogService,
            IRuntimeDataService runtimeDataService)
        {
            _dialogService = dialogService;
            _runtimeDataService = runtimeDataService;

            InitializeComponent();

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

            // Hold reference to FontAwesome library
            ImageAwesome.CreateImageSource(FontAwesomeIcon.Times, Brushes.Black);

            SizeChanged += (s, e) =>
            {
                if (WindowState == WindowState.Normal)
                {
                    WidthNormal = Width;
                    HeightNormal = Height;
                }
            };

            LocationChanged += (s, e) =>
            {
                if (WindowState == WindowState.Normal)
                {
                    TopNormal = Top;
                    LeftNormal = Left;
                }
            };

            Loaded += (s, e) =>
            {
                HwndSource.FromHwnd(new WindowInteropHelper(this).Handle).AddHook(new HwndSourceHook(WindowProc));

                DataContext = viewModel;

                if (viewModel != null)
                {
                    viewModel.Loaded();
                }

                LoadWindowState();
            };

            Closed += (s, e) =>
            {
                SaveWindowState();
            };
        }

        #endregion Constructors

        #region Properties

        public double WidthNormal { get; set; }

        public double HeightNormal { get; set; }

        public double TopNormal { get; set; }

        public double LeftNormal { get; set; }

        #endregion Properties

        #region Methods

        public void LoadWindowState()
        {
            try
            {
                MainWindowInfo mainWindowInfo = _runtimeDataService.RuntimeData.MainWindowInfo;

                if (mainWindowInfo != null)
                {
                    Width = Math.Max(MinWidth, mainWindowInfo.Width);
                    Height = Math.Max(MinHeight, mainWindowInfo.Height);
                    Top = mainWindowInfo.Top;
                    Left = mainWindowInfo.Left;
                    WindowState = mainWindowInfo.IsMaximized ? WindowState.Maximized : WindowState.Normal;
                    ValidateWindowState(false);
                }
                else
                {
                    ValidateWindowState(true);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        public void SaveWindowState()
        {
            try
            {
                MainWindowInfo mainWindowInfo = new MainWindowInfo()
                {
                    Width = WidthNormal,
                    Height = HeightNormal,
                    Top = TopNormal,
                    Left = LeftNormal,
                    IsMaximized = WindowState == WindowState.Maximized
                };

                _runtimeDataService.RuntimeData.MainWindowInfo = mainWindowInfo;
                _runtimeDataService.Save();
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void ValidateWindowState(bool firstStart)
        {
            if (firstStart)
            {
                Screen screen = Screen.FromHandle(new WindowInteropHelper(this).Handle);

                double availableHeight = screen.WorkingArea.Height;

                if (Height > availableHeight)
                {
                    Height = Math.Max(MinHeight, availableHeight);

                    if (Height > availableHeight)
                    {
                        Top = 0;
                    }
                    else
                    {
                        Top = (availableHeight / 2) - (Height / 2);
                    }
                }
            }
            else
            {
                Screen currentScreen = Screen.FromRectangle(new System.Drawing.Rectangle((int)Left, (int)Top, (int)Width, (int)Height));
                Screen mostRightScreen = Screen.AllScreens.Aggregate((s1, s2) => s1.Bounds.Right > s2.Bounds.Right ? s1 : s2);

                if (Top < 0)
                {
                    Top = 0;
                }

                if (Top > currentScreen.WorkingArea.Height)
                {
                    Top = Math.Max(0, currentScreen.WorkingArea.Height - Height);
                }

                if (Left < 0)
                {
                    Left = 0;
                }

                if (Left > mostRightScreen.Bounds.Right)
                {
                    Left = Math.Max(mostRightScreen.Bounds.Left, mostRightScreen.Bounds.Right - Width);
                }
            }
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handeled)
        {
            switch (msg)
            {
                case NativeFlags.WM_GETMINMAXINFO:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handeled = true;
                    break;

                case NativeFlags.WM_WINDOWPOSCHANGING:
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

                    int minWidth = (int)Math.Round(MinWidth / 96 * dpiX, 0);

                    if (pos.cx < minWidth)
                    {
                        pos.cx = minWidth;
                        changedPos = true;
                    }

                    int minHeight = (int)Math.Round(MinHeight / 96 * dpiY, 0);

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

            IntPtr hMonitor = MonitorFromWindowNative(hwnd, NativeFlags.MONITOR_DEFAULTTONEAREST);

            MonitorInfoEx info = new MonitorInfoEx();
            GetMonitorInfoNative(new HandleRef(null, hMonitor), info);

            NativeStructs.Rect rcWorkArea = info.rcWork;
            NativeStructs.Rect rcMonitorArea = info.rcMonitor;

            mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
            mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
            mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
            mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        public void ShowNotification(string text)
        {
            notificationStrip.ShowNotification(text);
        }

        #endregion Methods
    }
}