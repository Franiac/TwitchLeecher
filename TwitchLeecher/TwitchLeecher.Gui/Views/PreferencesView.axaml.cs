using Avalonia.Controls;
using Avalonia.Input;
using TwitchLeecher.Gui.ViewModels;

namespace TwitchLeecher.Gui.Views
{
    public partial class PreferencesView : UserControl
    {
        public PreferencesView()
        {
            InitializeComponent();
        }

        private void InputElement_OnGotFocus(object? sender, GotFocusEventArgs e)
        {
            var viewModel = (PreferencesViewVM)DataContext;
            viewModel.IsChannelDropDownOpen = true;
        }
    }
}