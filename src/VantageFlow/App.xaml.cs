using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using VantageFlow.Core;
using VantageFlow.Modules.Notes;
using VantageFlow.Modules.TaskManager;

namespace VantageFlow;

/// <summary>
/// Composition root. Builds the DI container, lets every module register its own services,
/// and starts/stops each module around the window's lifetime. Adding a module means adding
/// one entry to <see cref="Modules"/> — nothing else here changes.
/// </summary>
public partial class App : Application
{
    private static readonly IReadOnlyList<IAppModule> Modules =
    [
        new TaskManagerModule(),
        new NotesModule(),
    ];

    public IHost Host { get; }

    public static T GetService<T>() where T : class
    {
        if ((Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices.");

        return service;
    }

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton(Modules);
                services.AddSingleton<NavigationService>();
                services.AddSingleton<INavigationService>(sp => sp.GetRequiredService<NavigationService>());
                services.AddTransient<ShellViewModel>();

                foreach (var module in Modules)
                    module.RegisterServices(services);
            })
            .Build();
    }

    private Window? _window;

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        foreach (var module in Modules)
            await module.StartAsync(Host.Services);

        _window = new MainWindow();
        _window.Activate();
    }
}
