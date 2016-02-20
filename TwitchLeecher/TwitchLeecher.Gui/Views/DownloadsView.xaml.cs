using System.Windows.Controls;
using TwitchLeecher.Gui.ViewModels;

namespace TwitchLeecher.Gui.Views
{
    public partial class DownloadsView : UserControl
    {
        public DownloadsView(DownloadsVM viewModel)
        {
            InitializeComponent();

            this.DataContext = viewModel;
        }
    }
}