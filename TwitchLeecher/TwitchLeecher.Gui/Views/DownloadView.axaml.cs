using System;
using Avalonia.Controls;
using Avalonia.Threading;

namespace TwitchLeecher.Gui.Views
{
    public partial class DownloadView : UserControl
    {
        public DownloadView()
        {
            InitializeComponent();

            IsVisibleChanged += DownloadView_IsVisibleChanged;
        }

        private void DownloadView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                {
                    cmbQuality.Focus();
                }));
            }
        }
    }
}