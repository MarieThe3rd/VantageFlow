# Composition Root and Dependency Injection

**What was built**: `App.xaml.cs` as the DI composition root, using `Microsoft.Extensions.DependencyInjection` + `Microsoft.Extensions.Hosting` — the exact pattern from Microsoft's own [Architecture patterns for WinUI 3 desktop apps](https://learn.microsoft.com/windows/apps/develop/architecture-patterns).

## The code, annotated

```csharp
public partial class App : Application
{
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
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<NavigationService>();
                services.AddSingleton<INavigationService>(sp => sp.GetRequiredService<NavigationService>());
                services.AddTransient<ShellViewModel>();
                foreach (var module in Modules)
                    module.RegisterServices(services);
            })
            .Build();
    }
}
```

- **`Host.CreateDefaultBuilder().ConfigureServices(...).Build()`** is the same `Microsoft.Extensions.Hosting` pattern ASP.NET Core uses — WinUI has no built-in container, but nothing stops you from bringing this one. `Host` is built once, in the constructor, before any window exists.
- **`App.GetService<T>()`** is the bridge between "things DI constructs" and "things WinUI constructs for you." A `Page` is instantiated by `Frame.Navigate(typeof(Page))` via reflection — DI never sees it — so a Page's constructor calls `App.GetService<TViewModel>()` itself to obtain its ViewModel, rather than the ViewModel being handed to it. This is different from ASP.NET Core, where the framework constructs *everything* (controllers, services) through DI; here, only what you explicitly resolve goes through the container.
- **Registering the interface via a factory** (`AddSingleton<INavigationService>(sp => sp.GetRequiredService<NavigationService>())`) makes `NavigationService` and `INavigationService` resolve to the *same* singleton instance. This matters here specifically because `ShellPage` needs the concrete `NavigationService` (to call `.Initialize(frame)`, a method not on the interface), while ViewModels only ever see `INavigationService`.
- **`foreach (var module in Modules) module.RegisterServices(services)`** is the entire point of the `IAppModule` contract: adding a module means adding one entry to the `Modules` list (see `Documentation/Walkthroughs/03-navigation-shell-and-modules.md`) — this loop, and everything downstream of it, never needs to change.

## How it compares

If you've used `IServiceCollection`/`IServiceProvider` in ASP.NET Core, this is the identical API — same `AddSingleton`/`AddTransient`, same container. The difference is *who calls `GetService`*: in ASP.NET Core the framework does it for you on every request; here, you call `App.GetService<T>()` explicitly, once per Page, because WinUI's navigation system doesn't know DI exists.
