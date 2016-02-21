using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Unity;
using System;
using System.Windows;
using System.Windows.Threading;
using TwitchLeecher.Gui.Modules;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Services.Modules;

namespace TwitchLeecher
{
    public class Bootstrapper : UnityBootstrapper
    {
        #region Methods

        protected override void ConfigureModuleCatalog()
        {
            base.ConfigureModuleCatalog();

            ModuleCatalog mc = (ModuleCatalog)this.ModuleCatalog;

            mc.AddModule(typeof(GuiModule), nameof(ServiceModule));
            mc.AddModule(typeof(ServiceModule));
        }

        protected override DependencyObject CreateShell()
        {
            return this.Container.Resolve<Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Application.Current.MainWindow = (Window)this.Shell;
            Application.Current.MainWindow.Show();
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = e.Exception;

                ILogService logService = this.Container.Resolve<ILogService>();
                string logFile = logService.LogException(ex);

                MessageBox.Show("An unhandled UI exception occured and was written to log file"
                    + Environment.NewLine + Environment.NewLine + logFile
                    + Environment.NewLine + Environment.NewLine + "Application will now exit...",
                    "Fatal UI Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Application.Current.Shutdown();
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
                    Application.Current.Shutdown();
                }
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;

                ILogService logService = this.Container.Resolve<ILogService>();
                string logFile = logService.LogException(ex);

                MessageBox.Show("An unhandled exception occured and was written to a log file!"
                    + Environment.NewLine + Environment.NewLine + logFile
                    + Environment.NewLine + Environment.NewLine + "Application will now exit...",
                    "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Application.Current.Shutdown();
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
                    Application.Current.Shutdown();
                }
            }
        }

        #endregion Methods
    }
}