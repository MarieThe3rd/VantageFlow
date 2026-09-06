using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VantageFlow.Core;
using VantageFlow.Core.Modules.Notes.Data;
using VantageFlow.Core.Modules.Notes.Services;
using VantageFlow.Core.Modules.Notes.ViewModels;
using VantageFlow.Modules.Notes.Views;
using Windows.Storage;

namespace VantageFlow.Modules.Notes;

/// <summary>
/// The second module — deliberately simple, to prove IAppModule generalizes rather than to add
/// more of what TaskManager already demonstrates. Owns its own database file (see
/// Documentation/01-decisions-log.md), independent of TaskManager's.
/// </summary>
public sealed class NotesModule : IAppModule
{
    public void RegisterServices(IServiceCollection services)
    {
        var dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "notes.db");
        services.AddDbContextFactory<NotesDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

        services.AddTransient<INoteRepository, NoteRepository>();
        services.AddTransient<NotesViewModel>();
    }

    public IEnumerable<NavigationItem> GetNavigationItems()
    {
        yield return new NavigationItem("Notes", NavigationIcon.Document, typeof(NotesPage));
    }

    public async Task StartAsync(IServiceProvider services)
    {
        var contextFactory = services.GetRequiredService<IDbContextFactory<NotesDbContext>>();
        await using var db = await contextFactory.CreateDbContextAsync();
        await db.Database.MigrateAsync();
    }

    public Task StopAsync(IServiceProvider services) => Task.CompletedTask;
}
