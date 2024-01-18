using Avalonia.Controls;
using TwitchLeecher.Gui.Types;

namespace TwitchLeecher.Gui.Views;

public partial class MessageBoxWindow : Window
{
    public MessageBoxWindow(string message, string caption, MessageBoxButton buttons, MessageBoxImage icon)
    {
        InitializeComponent();
    }
}