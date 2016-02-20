using System.Windows.Controls;
using TwitchLeecher.Gui.ViewModels;

namespace TwitchLeecher.Gui.Views
{
    public partial class TitleBarView : UserControl
    {
        public TitleBarView(TitleBarVM viewModel)
        {
            InitializeComponent();

            this.DataContext = viewModel;
        }
    }
}