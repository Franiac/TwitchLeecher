using System.Windows.Controls;
using TwitchLeecher.Gui.ViewModels;

namespace TwitchLeecher.Gui.Views
{
    public partial class WelcomeView : UserControl
    {
        public WelcomeView(WelcomeVM viewModel)
        {
            InitializeComponent();

            this.DataContext = viewModel;
        }
    }
}