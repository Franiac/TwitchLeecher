using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using System.Windows.Threading;

namespace TwitchLeecher.Gui.Views
{
    public partial class SearchWindow : Window
    {
        public SearchWindow()
        {
            InitializeComponent();

            WindowChrome windowChrome = new WindowChrome()
            {
                CaptionHeight = 51,
                CornerRadius = new CornerRadius(0),
                GlassFrameThickness = new Thickness(0),
                NonClientFrameEdges = NonClientFrameEdges.None,
                ResizeBorderThickness = new Thickness(0),
                UseAeroCaptionButtons = false
            };

            WindowChrome.SetWindowChrome(this, windowChrome);

            this.Loaded += SearchRequestView_Loaded;
        }

        private void SearchRequestView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.txtUsername.Focus();

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(() =>
            {
                this.txtUsername.SelectAll();
            }));
        }

        private void txtLoadLimit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int limit;

            if (!int.TryParse(e.Text, out limit))
            {
                e.Handled = true;
            }
        }
    }
}