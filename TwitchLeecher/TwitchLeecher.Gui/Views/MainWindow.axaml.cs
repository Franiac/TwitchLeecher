using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Gui.ViewModels;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Communication;
using TwitchLeecher.Shared.Native;
using static TwitchLeecher.Shared.Native.NativeMethods;
using static TwitchLeecher.Shared.Native.NativeStructs;

namespace TwitchLeecher.Gui.Views
{
    public partial class MainWindow : Window
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly IRuntimeDataService _runtimeDataService;
        private readonly NamedPipeManager _namedPipeManager;

        private bool _shown = false;

        #endregion Fields

        #region Constructors

        public MainWindow(
            MainWindowVM viewModel,
            IDialogService dialogService,
            IRuntimeDataService runtimeDataService)
        {
            _dialogService = dialogService;
            _runtimeDataService = runtimeDataService;

            _namedPipeManager = new NamedPipeManager("TwitchLeecher-DX");
            _namedPipeManager.OnMessage += OnPipeMessage;
            _namedPipeManager.StartServer();

            InitializeComponent();

            SizeChanged += (s, e) =>
            {
                if (WindowState == WindowState.Normal)
                {
                    WidthNormal = Width;
                    HeightNormal = Height;
                }
            };

            Loaded += (s, e) =>
            {
                DataContext = viewModel;

                if (viewModel != null)
                {
                    viewModel.Loaded();
                }

                LoadWindowState();
            };

            Activated += (s, e) =>
            {
                if (viewModel != null && !_shown)
                {
                    _shown = true;
                    viewModel.Shown();
                }
            };

            Closed += (s, e) =>
            {
                SaveWindowState();

                _namedPipeManager.StopServer();
            };
        }

        #endregion Constructors

        #region Properties

        public double WidthNormal { get; set; }

        public double HeightNormal { get; set; }

        public double TopNormal { get; set; }

        public double LeftNormal { get; set; }

        #endregion Properties

        #region Methods

        private void OnPipeMessage(string message)
        {
            if (message == "Activate")
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    if (WindowState == WindowState.Minimized)
                    {
                        WindowState = WindowState.Normal;
                    }

                    this.Topmost = true;

                    this.Activate();

                    Dispatcher.UIThread.InvokeAsync(() => { this.Topmost = false; });
                });
            }
        }

        public void LoadWindowState()
        {
            try
            {
                MainWindowInfo mainWindowInfo = _runtimeDataService.RuntimeData.MainWindowInfo;

                if (mainWindowInfo != null)
                {
                    Width = Math.Max(MinWidth, mainWindowInfo.Width);
                    Height = Math.Max(MinHeight, mainWindowInfo.Height);
                    Margin = new Thickness(mainWindowInfo.Left, mainWindowInfo.Top, 0, 0);
                    WindowState = mainWindowInfo.IsMaximized ? WindowState.Maximized : WindowState.Normal;
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        public void SaveWindowState()
        {
            try
            {
                MainWindowInfo mainWindowInfo = new MainWindowInfo()
                {
                    Width = WidthNormal,
                    Height = HeightNormal,
                    Top = TopNormal,
                    Left = LeftNormal,
                    IsMaximized = WindowState == WindowState.Maximized
                };

                _runtimeDataService.RuntimeData.MainWindowInfo = mainWindowInfo;
                _runtimeDataService.Save();
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        #endregion Methods

    }
}