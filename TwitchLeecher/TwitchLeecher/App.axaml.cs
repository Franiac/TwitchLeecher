using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Ninject;
using TwitchLeecher.Gui.Modules;
using TwitchLeecher.Gui.ViewModels;
using TwitchLeecher.Gui.Views;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Services.Modules;
using TwitchLeecher.Services.Services;
using TwitchLeecher.Shared.Communication;
using TwitchLeecher.Shared.Events;

namespace TwitchLeecher;

public partial class App : Application
{
    // private readonly Mutex _singletonMutex;
    //
    // private IKernel _kernel;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // public App()
    // {
    //     _singletonMutex = new Mutex(true, "TwitchLeecher", out bool isOnlyInstance);
    //
    //     if (!isOnlyInstance)
    //     {
    //         new NamedPipeManager("TwitchLeecher").Write("Activate");
    //
    //         Environment.Exit(0);
    //     }
    // }
    //
    // private void OnExit(object s, ControlledApplicationLifetimeExitEventArgs e)
    // {
    //     if (_singletonMutex != null)
    //     {
    //         _singletonMutex.Dispose();
    //     }
    // }
    //
    // private void SetTheme(string name)
    // {
    //     // var currentStyle = Styles.FirstOrDefault();
    //     // Styles.Add(new ResourceDictionary
    //     // {
    //     //     MergedDictionaries =
    //     //     {
    //     //         new ResourceInclude(new Uri(
    //     //             $"pack://application:,,,/TwitchLeecher.Gui;component/Theme/{name}/Style.xaml",
    //     //             UriKind.Absolute))
    //     //     }
    //     // });
    //     // if (currentStyle != null) Styles.Remove(currentStyle);
    // }
    //
    // // private IList<IResourceProvider> Styles => Resources.MergedDictionaries;
    //
    // private IKernel CreateKernel()
    // {
    //     IKernel kernel = new StandardKernel();
    //
    //     RegisterTypes(kernel);
    //     LoadModules(kernel);
    //
    //     return kernel;
    // }
    //
    // private void RegisterTypes(IKernel kernel)
    // {
    //     kernel.Bind<MainWindow>().ToSelf().InSingletonScope();
    //     kernel.Bind<MainWindowVM>().ToSelf().InSingletonScope();
    //     kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
    // }
    //
    // private void LoadModules(IKernel kernel)
    // {
    //     kernel.Load<GuiModule>();
    //     kernel.Load<ServiceModule>();
    // }

    public override void OnFrameworkInitializationCompleted()
    {
        // _kernel = CreateKernel();
        // var themeService = _kernel.Get<IThemeService>();
        // themeService.StyleChanged += (sender, args) => { SetTheme(themeService.GetTheme()); };
        // var preferencesService = _kernel.Get<IPreferencesService>();
        // SetTheme(string.IsNullOrEmpty(preferencesService.CurrentPreferences.Theme)
        //     ? "New"
        //     : preferencesService.CurrentPreferences.Theme);
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new TestMainWindow();
            // desktop.MainWindow = _kernel.Get<MainWindow>();
            // desktop.Exit += OnExit;
        }


        base.OnFrameworkInitializationCompleted();
    }
}