using Avalonia.Controls;
using TwitchLeecher.Gui.Types;
using TwitchLeecher.Gui.ViewModels;

namespace TwitchLeecher.Gui.Views
{
    public partial class MessageBoxWindow : Window
    {
        #region Constructors

        public MessageBoxWindow(string message, string caption = null,
            MessageBoxButton messageBoxButton = MessageBoxButton.OK,
            MessageBoxImage image = MessageBoxImage.Information)
        {
            var viewModel = new MessageBoxViewModel(result => Complete(result))
            {
                Message = message,
                Caption = caption
            };
            viewModel.SetIcon(image);
            viewModel.SetButtons(messageBoxButton);
            DataContext = viewModel;
        }

        private void Complete(MessageBoxResult result)
        {
            Result = result;
            Close();
        }

        #endregion Constructors

        public MessageBoxResult Result { get; set; }
    }
}