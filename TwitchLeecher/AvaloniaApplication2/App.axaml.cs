using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Ninject;
using TwitchLeecher.Gui.Modules;
using TwitchLeecher.Gui.ViewModels;
using TwitchLeecher.Gui.Views;
using TwitchLeecher.Services.Modules;
using TwitchLeecher.Shared.Events;

namespace AvaloniaApplication2;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

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
    public override void OnFrameworkInitializationCompleted()
    {
        var kernel = CreateKernel();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = kernel.Get<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}