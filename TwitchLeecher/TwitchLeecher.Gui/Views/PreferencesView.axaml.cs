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
            var viewModel = (PreferencesViewModel)DataContext;
            viewModel.IsChannelDropDownOpen = true;
        }
    }
}