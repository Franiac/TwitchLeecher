using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TwitchLeecher.Gui.Views
{
    public partial class DownloadView : UserControl
    {
        public DownloadView()
        {
            InitializeComponent();

            this.IsVisibleChanged += this.DownloadView_IsVisibleChanged;
        }

        private void DownloadView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                {
                    this.cmbQuality.Focus();
                }));
            }
        }
    }
}