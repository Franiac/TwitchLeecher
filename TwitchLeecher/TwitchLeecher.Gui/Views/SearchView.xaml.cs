using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TwitchLeecher.Gui.Views
{
    public partial class SearchView : UserControl
    {
        public SearchView()
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
                    cmbChannel.Focus();
                }));
            }
        }
    }
}