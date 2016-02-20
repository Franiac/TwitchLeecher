using System.Windows;

namespace TwitchLeecher.Setup.Gui.Services
{
    internal interface IGuiService
    {
        void SetBusy();

        MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton button, MessageBoxImage image);
    }
}