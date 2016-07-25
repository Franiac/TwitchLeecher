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

            this.IsVisibleChanged += this.SearchView_IsVisibleChanged;
        }

        private void SearchView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                {
                    this.txtLog.CaretIndex = this.txtLog.Text.Length;
                    this.txtLog.ScrollToEnd();
                }));
            }
        }
    }
}