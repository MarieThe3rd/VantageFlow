using Microsoft.Extensions.DependencyInjection;
using VantageFlow.Core;
using VantageFlow.Core.Modules.TaskManager.ViewModels;
using VantageFlow.Modules.TaskManager.Views;

namespace VantageFlow.Modules.TaskManager;

public sealed class TaskManagerModule : IAppModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<TasksViewModel>();
    }

    public IEnumerable<NavigationItem> GetNavigationItems()
    {
        yield return new NavigationItem("Tasks", NavigationIcon.List, typeof(TasksPage));
    }

    public Task StartAsync(IServiceProvider services) => Task.CompletedTask;

    public Task StopAsync(IServiceProvider services) => Task.CompletedTask;
}
