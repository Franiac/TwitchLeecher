using Avalonia.Controls;
using TwitchLeecher.Gui.Interfaces;

namespace TwitchLeecher.Gui.Views
{
    public partial class DownloadsView : UserControl
    {
        #region Fields

        private INavigationState _state;

        #endregion Fields

        #region Constructors

        public DownloadsView()
        {
            InitializeComponent();
        }

        #endregion Constructors

    }
}