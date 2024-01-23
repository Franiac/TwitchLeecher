using Avalonia.Controls;
using Avalonia.Input;
using TwitchLeecher.Gui.ViewModels;

namespace TwitchLeecher.Gui.Views
{
    public partial class SearchView : UserControl
    {
        public SearchView()
        {
            InitializeComponent();
        }

        private void InputElement_OnGotFocus(object? sender, GotFocusEventArgs e)
        {
            var viewModel = (SearchViewModel)DataContext;
            viewModel.OpenSearchDropDown = true;
        }
    }
}