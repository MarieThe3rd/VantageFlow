using Microsoft.Extensions.DependencyInjection;

namespace VantageFlow.Core;

/// <summary>
/// A self-contained feature area (e.g. Task Manager). Implementations register their own
/// services and contribute their own navigation entries — adding a module never requires
/// editing the shell or the composition root.
/// </summary>
public interface IAppModule
{
    void RegisterServices(IServiceCollection services);

    IEnumerable<NavigationItem> GetNavigationItems();

    Task StartAsync(IServiceProvider services);

    Task StopAsync(IServiceProvider services);
}
