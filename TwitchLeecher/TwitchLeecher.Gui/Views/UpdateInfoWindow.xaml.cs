using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;
using TwitchLeecher.Gui.Services;

namespace TwitchLeecher.Gui.Views
{
    public partial class UpdateInfoWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;

        private IGuiService guiService;

        public UpdateInfoWindow(IGuiService guiService)
        {
            this.guiService = guiService;

            InitializeComponent();

            WindowChrome windowChrome = new WindowChrome()
            {
                CaptionHeight = 51,
                CornerRadius = new CornerRadius(0),
                GlassFrameThickness = new Thickness(0),
                NonClientFrameEdges = NonClientFrameEdges.None,
                ResizeBorderThickness = new Thickness(6),
                UseAeroCaptionButtons = false
            };

            WindowChrome.SetWindowChrome(this, windowChrome);

            this.SourceInitialized += LogWindow_SourceInitialized;
        }

        private void LogWindow_SourceInitialized(object sender, System.EventArgs e)
        {
            try
            {
                var hwnd = new WindowInteropHelper((Window)sender).Handle;
                var value = GetWindowLong(hwnd, GWL_STYLE);
                SetWindowLong(hwnd, GWL_STYLE, (int)(value & ~WS_MAXIMIZEBOX));
            }
            catch (Exception ex)
            {
                this.guiService.ShowAndLogException(ex);
            }
        }
    }
}