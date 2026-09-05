using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VantageFlow.Core;
using VantageFlow.Core.Modules.TaskManager.Data;
using VantageFlow.Core.Modules.TaskManager.Services;
using VantageFlow.Core.Modules.TaskManager.ViewModels;
using VantageFlow.Modules.TaskManager.Views;
using Windows.Storage;

namespace VantageFlow.Modules.TaskManager;

public sealed class TaskManagerModule : IAppModule
{
    public void RegisterServices(IServiceCollection services)
    {
        var dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "vantageflow.db");
        services.AddDbContextFactory<TaskManagerDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

        services.AddTransient<ITaskRepository, TaskRepository>();
        services.AddTransient<IPersonRepository, PersonRepository>();
        services.AddTransient<TasksViewModel>();
    }

    public IEnumerable<NavigationItem> GetNavigationItems()
    {
        yield return new NavigationItem("Tasks", NavigationIcon.List, typeof(TasksPage));
    }

    public async Task StartAsync(IServiceProvider services)
    {
        // Applies any pending migrations, creating the database on first run.
        var contextFactory = services.GetRequiredService<IDbContextFactory<TaskManagerDbContext>>();
        await using var db = await contextFactory.CreateDbContextAsync();
        await db.Database.MigrateAsync();
    }

    public Task StopAsync(IServiceProvider services) => Task.CompletedTask;
}
