using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Windows;

namespace TwitchLeecher.Gui.Interfaces
{
    public interface IDialogService
    {
        MessageBoxResult ShowMessageBox(string message);

        MessageBoxResult ShowMessageBox(string message, string caption);

        MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton buttons);

        MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton buttons, MessageBoxImage icon);

        void ShowAndLogException(Exception ex);

        void ShowFolderBrowserDialog(string folder, Action<bool, string> dialogCompleteCallback);

        void ShowFileBrowserDialog(CommonFileDialogFilter filter, string folder, Action<bool, string> dialogCompleteCallback);

        void SetBusy();
    }
}