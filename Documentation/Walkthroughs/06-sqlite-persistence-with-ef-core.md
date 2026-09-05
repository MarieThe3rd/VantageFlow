# SQLite Persistence with EF Core

**What was built**: `TaskManagerDbContext`, `ITaskRepository`/`TaskRepository`, `IPersonRepository`/`PersonRepository`, an EF Core migration, and startup wiring in `TaskManagerModule` — all in `VantageFlow.Core`, since EF Core has no WinUI dependency and can live right alongside the Models it persists.

## Why `IDbContextFactory<T>`, not the usual `AddDbContext`

Most EF Core examples register a `DbContext` with `AddDbContext`, which defaults to a **Scoped** lifetime — one instance per web request in ASP.NET Core. A desktop app has no request, so there's no natural scope boundary for that instance to live and die with. Microsoft's own guidance for exactly this situation (desktop apps, Blazor Server, anything without per-request scopes) is `AddDbContextFactory`:

```csharp
services.AddDbContextFactory<TaskManagerDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));
```

Repositories take an `IDbContextFactory<TaskManagerDbContext>` and call `CreateDbContextAsync()` per operation, disposing it immediately after (`await using`). Each repository call gets its own short-lived context — safe, since EF Core `DbContext` instances are explicitly **not** thread-safe and were never meant to be held open indefinitely.

## Shadow properties: one entity, two relationships

`TaskItem` has *two* `Person?` properties — `Requester` and `Recipient`. EF Core's convention-based relationship discovery can't tell which foreign key column belongs to which navigation when there are two references to the same entity type; it needs to be told explicitly:

```csharp
task.HasOne(t => t.Requester).WithMany().HasForeignKey("RequesterId");
task.HasOne(t => t.Recipient).WithMany().HasForeignKey("RecipientId");
```

`"RequesterId"` and `"RecipientId"` are **shadow properties** — they exist as real columns in the SQLite table, but there's no matching C# property on `TaskItem` itself. EF Core tracks them internally. This is deliberate: `TaskItem` stays exactly what `CONTEXT.md` says it is (a `Requester`, not a `RequesterId`) — the foreign-key plumbing is a `Data/` concern, not a domain one.

## The disconnected-entity gotcha

This one showed up as a genuine question worth asking before assuming: *if a `Person` was already saved (has a real database `Id`), what happens when a new `Task` referencing that same `Person` object gets saved through a completely different `DbContext` instance?*

The answer, without intervention: EF Core doesn't know that `Person` came from the database — as far as this new, empty-tracking-list `DbContext` is concerned, it's just an object it's never seen, so `SaveChanges` would try to `INSERT` a second, duplicate row for a person who already exists. The fix:

```csharp
private static void AttachIfExisting<TEntity>(TaskManagerDbContext db, TEntity? entity) where TEntity : class
{
    if (entity is not null && db.Entry(entity).State == EntityState.Detached)
    {
        db.Attach(entity);   // marks it Unchanged — "this already exists, just reference it"
    }
}
```

Called once for each of `Requester`, `Recipient`, `Project`, `Source` before `db.Tasks.Add(task)`. `TaskRepositoryTests.AddAsync_PersistsTaskWithAnAlreadySavedRequester_WithoutDuplicatingThePerson` exercises this against a **real** temp-file SQLite database (not a fake) specifically because this is exactly the kind of bug that a fake repository would never be able to catch — the behavior is inherent to how EF Core's change tracker works across multiple `DbContext` instances, not something a hand-written fake would ever reproduce.

## Migrations: authored against Core, run against the app

`dotnet-ef` needs a project it can actually execute to discover the model, but `VantageFlow.Core` is a library — running the tool directly against it failed (`UseWinUI=true` still pulls in a Windows SDK runtime pack the design-time host can't resolve alone). The fix was `dotnet ef`'s own `--project`/`--startup-project` split:

```
dotnet ef migrations add InitialCreate --project src/VantageFlow.Core/VantageFlow.Core.csproj --startup-project src/VantageFlow/VantageFlow.csproj
```

`--project` says where the `DbContext` (and the generated migration files) live; `--startup-project` says which project actually has a complete, runnable dependency set to host the design-time tooling. Both projects needed a direct `Microsoft.EntityFrameworkCore.Design` reference (`PrivateAssets="all"` — a dev-time-only tool, never shipped) — the CLI checks the *startup* project's own package references directly, not what flows to it transitively through a `ProjectReference`.

At runtime, there's no separate "run this installer" step: `TaskManagerModule.StartAsync` (part of the `IAppModule` contract from day one, see `Documentation/Walkthroughs/03`) calls `Database.MigrateAsync()` once, on every launch — creating the database on first run and applying anything new on every run after.

## A small test-only gotcha: connection pooling

The first version of `TaskRepositoryTests` failed — not on any assertion, but in cleanup: deleting the temp `.db` file threw "being used by another process." `Microsoft.Data.Sqlite` pools connections by connection string, so a file handle stays open even after every `DbContext` pointing at it has been disposed. `SqliteConnection.ClearAllPools()` before deleting the file is the fix — a test-infrastructure detail, not an application bug, but worth knowing if a SQLite-backed test's teardown ever mysteriously can't delete its own database file.
