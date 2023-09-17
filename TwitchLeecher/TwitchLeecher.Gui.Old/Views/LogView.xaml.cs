using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TwitchLeecher.Gui.Views
{
    public partial class LogView : UserControl
    {
        public LogView()
        {
            InitializeComponent();

            IsVisibleChanged += SearchView_IsVisibleChanged;
        }

        private void SearchView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                {
                    txtLog.CaretIndex = txtLog.Text.Length;
                    txtLog.ScrollToEnd();
                }));
            }
        }
    }
}