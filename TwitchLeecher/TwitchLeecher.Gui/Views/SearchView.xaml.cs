using System;
using System.Globalization;
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

            dtLoadFrom.FormatString = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            dtLoadTo.FormatString = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;

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