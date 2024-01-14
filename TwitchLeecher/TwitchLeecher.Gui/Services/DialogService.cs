using Ninject;
using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Gui.Types;
using TwitchLeecher.Gui.ViewModels;
using TwitchLeecher.Gui.Views;
using TwitchLeecher.Services.Interfaces;

namespace TwitchLeecher.Gui.Services
{
    internal class DialogService : IDialogService
    {
        #region Fields

        private readonly IKernel _kernel;
        private readonly ILogService _logService;

        private bool _busy;

        #endregion Fields

        #region Constructor

        public DialogService(IKernel kernel, ILogService logService)
        {
            _kernel = kernel;
            _logService = logService;
        }

        #endregion Constructor

        #region Methods

        public MessageBoxResult ShowMessageBox(string message)
        {
            MessageBoxWindow msg = new MessageBoxWindow(message);
            return SetOwnerAndShow(msg);
        }

        public MessageBoxResult ShowMessageBox(string message, string caption)
        {
            MessageBoxWindow msg = new MessageBoxWindow(message, caption);
            return SetOwnerAndShow(msg);
        }

        public MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton buttons)
        {
            MessageBoxWindow msg = new MessageBoxWindow(message, caption, buttons);
            return SetOwnerAndShow(msg);
        }

        public MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton buttons,
            MessageBoxImage icon)
        {
            MessageBoxWindow msg = new MessageBoxWindow(message, caption, buttons, icon);
            return SetOwnerAndShow(msg);
        }

        private MessageBoxResult SetOwnerAndShow(MessageBoxWindow msg)
        {
            msg.ShowDialog(_kernel.Get<MainWindow>());

            return msg.Result;
        }

        public void ShowAndLogException(Exception ex)
        {
            if (ex == null)
            {
                return;
            }

            string logFile = _logService.LogException(ex);

            MessageBoxWindow msg = new MessageBoxWindow("An unexpected error occured:"
                                                        + Environment.NewLine + Environment.NewLine + ex.Message
                                                        + Environment.NewLine + Environment.NewLine +
                                                        "All details were written to log file"
                                                        + Environment.NewLine + Environment.NewLine + logFile,
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            msg.ShowDialog(_kernel.Get<MainWindow>());
        }

        public void ShowFolderBrowserDialog(string folder, Action<bool, string> dialogCompleteCallback)
        {
            var window = _kernel.Get<MainWindow>();
            var picker = window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                AllowMultiple = false
            }).GetAwaiter().GetResult();
            if (picker.Any())
            {
                dialogCompleteCallback(true, picker.First().Path.AbsolutePath);
                return;
            }

            dialogCompleteCallback(false, null);
        }

        public void ShowFileBrowserDialog(CommonFileDialogFilter filter, string folder,
            Action<bool, string> dialogCompleteCallback)
        {
            var window = _kernel.Get<MainWindow>();
            var result = window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                SuggestedStartLocation =
                    window.StorageProvider.TryGetFolderFromPathAsync(folder).GetAwaiter().GetResult(),
                FileTypeFilter = new[]
                    { new FilePickerFileType(filter.Name) { Patterns = new[] { $"*.{filter.Extension}" } } }
            }).GetAwaiter().GetResult();
            if (result.Any())
            {
                dialogCompleteCallback(true, result.First().Path.AbsolutePath);
                return;
            }

            dialogCompleteCallback(false, null);
        }

        public void ShowUpdateInfoDialog(UpdateInfo updateInfo)
        {
            UpdateInfoViewVM vm = _kernel.Get<UpdateInfoViewVM>();
            var mainWindow = _kernel.Get<MainWindow>();
            vm.UpdateInfo = updateInfo;

            UpdateInfoView view = new UpdateInfoView
            {
                DataContext = vm
            };

            view.ShowDialog(mainWindow);
        }

        public void SetBusy()
        {
            SetBusy(true);
        }

        private void SetBusy(bool busy)
        {
            if (_busy != busy)
            {
                _busy = busy;

                var window = _kernel.Get<MainWindow>();
                window.Cursor = busy ? new Cursor(StandardCursorType.Wait) : Cursor.Default;
                if (_busy)
                {
                    new DispatcherTimer(TimeSpan.FromSeconds(0), DispatcherPriority.ApplicationIdle,
                        DispatcherTimer_Tick);
                }
            }
        }

        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            if (sender is DispatcherTimer dispatcherTimer)
            {
                SetBusy(false);
                dispatcherTimer.Stop();
            }
        }

        #endregion Methods
    }
}