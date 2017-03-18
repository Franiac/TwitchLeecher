using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Gui.Views
{
    public partial class SearchView : UserControl
    {
        public SearchView()
        {
            InitializeComponent();

            cmbLoadLimit.ItemsSource = Preferences.GetLoadLimits();

            IsVisibleChanged += SearchView_IsVisibleChanged;
        }

        private void SearchView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                {
                    txtChannel.Focus();
                    txtChannel.SelectAll();
                }));
            }
        }
    }
}