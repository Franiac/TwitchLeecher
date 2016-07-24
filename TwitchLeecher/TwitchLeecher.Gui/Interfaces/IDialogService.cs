using System;
using System.Windows;
using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Gui.Interfaces
{
    public interface IDialogService
    {
        MessageBoxResult ShowMessageBox(string message);

        MessageBoxResult ShowMessageBox(string message, string caption);

        MessageBoxResult ShowMessageBox(Window owner, string message);

        MessageBoxResult ShowMessageBox(Window owner, string message, string caption);

        MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton buttons);

        MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton buttons, MessageBoxImage icon);

        void ShowAndLogException(Exception ex);

        void ShowSearchDialog(SearchParameters lastSearchParams, Action<bool, SearchParameters> dialogCompleteCallback);

        void ShowDownloadDialog(DownloadParameters downloadParams, Action<bool, DownloadParameters> dialogCompleteCallback);

        void ShowFolderBrowserDialog(string folder, Action<bool, string> dialogCompleteCallback);

        void ShowSaveFileDialog(string filename, Action<bool, string> dialogCompleteCallback);

        void ShowUpdateInfoWindow(UpdateInfo updateInfo);

        void ShowLog(TwitchVideoDownload download);

        void SetBusy();
    }
}