using Ninject;
using System;
using System.Windows;
using System.Windows.Threading;
using TwitchLeecher.Gui.Modules;
using TwitchLeecher.Gui.Views;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Services.Modules;
using TwitchLeecher.Shared.Events;

namespace TwitchLeecher
{
    public partial class App : Application
    {
        private IKernel kernel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.kernel = this.CreateKernel();

            this.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            this.MainWindow = this.kernel.Get<MainWindow>();
            this.MainWindow.Show();
        }

        private IKernel CreateKernel()
        {
            IKernel kernel = new StandardKernel();

            this.RegisterTypes(kernel);
            this.LoadModules(kernel);

            return kernel;
        }

        private void RegisterTypes(IKernel kernel)
        {
            kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
        }

        private void LoadModules(IKernel kernel)
        {
            kernel.Load<GuiModule>();
            kernel.Load<ServiceModule>();
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = e.Exception;

                ILogService logService = this.kernel.Get<ILogService>();
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

                ILogService logService = this.kernel.Get<ILogService>();
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
    }
}