using System.Windows.Controls;
using TwitchLeecher.Gui.ViewModels;

namespace TwitchLeecher.Gui.Views
{
    public partial class VideosView : UserControl
    {
        public VideosView(VideosVM viewModel)
        {
            InitializeComponent();

            this.DataContext = viewModel;
        }
    }
}