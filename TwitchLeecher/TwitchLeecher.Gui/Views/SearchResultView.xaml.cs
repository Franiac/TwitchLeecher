using System.Windows;
using System.Windows.Controls;
using TwitchLeecher.Gui.Interfaces;

namespace TwitchLeecher.Gui.Views
{
    public partial class SearchResultView : UserControl
    {
        #region Fields

        private INavigationState state;

        #endregion Fields

        #region Constructors

        public SearchResultView()
        {
            InitializeComponent();

            this.scroller.ScrollChanged += this.Scroller_ScrollChanged;
            this.Loaded += this.SearchResultView_Loaded;
        }

        #endregion Constructors

        #region EventHandlers

        private void Scroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (this.state != null)
            {
                state.ScrollPosition = e.VerticalOffset;
            }
        }

        private void SearchResultView_Loaded(object sender, RoutedEventArgs e)
        {
            this.state = this.DataContext as INavigationState;

            if (state != null)
            {
                this.scroller.ScrollToVerticalOffset(state.ScrollPosition);
            }
        }

        #endregion EventHandlers
    }
}