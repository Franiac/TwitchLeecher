using System.Windows;
using System.Windows.Controls;
using TwitchLeecher.Gui.Interfaces;

namespace TwitchLeecher.Gui.Views
{
    public partial class SearchResultView : UserControl
    {
        #region Fields

        private INavigationState _state;

        #endregion Fields

        #region Constructors

        public SearchResultView()
        {
            InitializeComponent();

            scroller.ScrollChanged += Scroller_ScrollChanged;
            Loaded += SearchResultView_Loaded;
        }

        #endregion Constructors

        #region EventHandlers

        private void Scroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_state != null)
            {
                _state.ScrollPosition = e.VerticalOffset;
            }
        }

        private void SearchResultView_Loaded(object sender, RoutedEventArgs e)
        {
            _state = DataContext as INavigationState;

            if (_state != null)
            {
                scroller.ScrollToVerticalOffset(_state.ScrollPosition);
            }
        }

        #endregion EventHandlers
    }
}