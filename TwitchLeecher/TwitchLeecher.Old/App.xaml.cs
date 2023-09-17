using Ninject;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using TwitchLeecher.Gui.Modules;
using TwitchLeecher.Gui.ViewModels;
using TwitchLeecher.Gui.Views;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Services.Modules;
using TwitchLeecher.Services.Services;
using TwitchLeecher.Shared.Communication;
using TwitchLeecher.Shared.Events;

namespace TwitchLeecher
{
    public partial class App : Application
    {
        #region Fields

        private readonly Mutex _singletonMutex;

        private IKernel _kernel;

        #endregion Fields

        #region Constructors

        public App()
        {
            _singletonMutex = new Mutex(true, "TwitchLeecher.Old", out bool isOnlyInstance);

            if (!isOnlyInstance)
            {
                new NamedPipeManager("TwitchLeecher.Old").Write("Activate");

                Environment.Exit(0);
            }
        }

        #endregion Constructors

        #region Methods

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            if (_singletonMutex != null)
            {
                _singletonMutex.Dispose();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (CultureInfo.CurrentCulture.Name.StartsWith("ar", StringComparison.OrdinalIgnoreCase) ||
                CultureInfo.CurrentUICulture.Name.StartsWith("ar", StringComparison.OrdinalIgnoreCase))
            {
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-US");
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            }

            _kernel = CreateKernel();

            DispatcherUnhandledException += Current_DispatcherUnhandledException;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject),
                new FrameworkPropertyMetadata(int.MaxValue));

            var themeService = _kernel.Get<IThemeService>();
            themeService.StyleChanged += (sender, args) => { SetTheme(themeService.GetTheme()); };
            var preferencesService = _kernel.Get<IPreferencesService>();
            SetTheme(string.IsNullOrEmpty(preferencesService.CurrentPreferences.Theme)
                ? "New"
                : preferencesService.CurrentPreferences.Theme);

            MainWindow = _kernel.Get<MainWindow>();
            MainWindow.Show();
        }

        private void SetTheme(string name)
        {
            var currentStyle = Styles.FirstOrDefault();
            Styles.Add(new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/TwitchLeecher.Old.Gui.Old;component/Theme/{name}/Style.xaml",
                    UriKind.Absolute)
            });
            Styles.Remove(currentStyle);
        }

        private Collection<ResourceDictionary> Styles => Resources.MergedDictionaries[0].MergedDictionaries;

        private IKernel CreateKernel()
        {
            IKernel kernel = new StandardKernel();

            RegisterTypes(kernel);
            LoadModules(kernel);

            return kernel;
        }

        private void RegisterTypes(IKernel kernel)
        {
            kernel.Bind<MainWindow>().ToSelf().InSingletonScope();
            kernel.Bind<MainWindowVM>().ToSelf().InSingletonScope();
            kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
        }

        private void LoadModules(IKernel kernel)
        {
            kernel.Load<GuiModule>();
            kernel.Load<ServiceModule>();
        }

        #endregion Methods

        #region EventHandler

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = e.Exception;

                ILogService logService = _kernel.Get<ILogService>();
                string logFile = logService.LogException(ex);

                MessageBox.Show("An unhandled UI exception occured and was written to log file"
                                + Environment.NewLine + Environment.NewLine + logFile
                                + Environment.NewLine + Environment.NewLine + "Application will now exit...",
                    "Fatal UI Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Current?.Shutdown();
            }
            catch
            {
                try
                {
                    MessageBox.Show("An unhandled UI exception occured but could not be written to a log file!"
                                    + Environment.NewLine + Environment.NewLine + "Application will now exit...",
                        "Fatal UI Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Current?.Shutdown();
                }
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;

                ILogService logService = _kernel.Get<ILogService>();
                string logFile = logService.LogException(ex);

                MessageBox.Show("An unhandled exception occured and was written to a log file!"
                                + Environment.NewLine + Environment.NewLine + logFile
                                + Environment.NewLine + Environment.NewLine + "Application will now exit...",
                    "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Current?.Shutdown();
            }
            catch
            {
                try
                {
                    MessageBox.Show("An unhandled exception occured but could not be written to a log file!"
                                    + Environment.NewLine + Environment.NewLine + "Application will now exit...",
                        "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Current?.Shutdown();
                }
            }
        }

        #endregion EventHandler
    }
}