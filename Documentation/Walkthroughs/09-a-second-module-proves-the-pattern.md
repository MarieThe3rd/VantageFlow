# A Second Module: Notes

**What was built**: the `Notes` module — `Note` (Model), `NotesDbContext`/`NoteRepository` (its own SQLite file, `notes.db`), `NotesViewModel`, `NotesPage`/`AddNoteDialog`, and `NotesModule : IAppModule` — deliberately as simple as a module can be, specifically to test whether the architecture from `Documentation/Walkthroughs/01`–`04` actually holds up for a *second* feature, not just describes the first one.

## What changed outside the Notes folder

One line, in `App.xaml.cs`:

```csharp
private static readonly IReadOnlyList<IAppModule> Modules =
[
    new TaskManagerModule(),
    new NotesModule(),
];
```

That's the entire integration. `ShellPage` didn't change — it already builds its `NavigationView` menu from whatever `IAppModule.GetNavigationItems()` returns across every registered module, so "Notes" just appears as a second entry the moment `NotesModule` is in the list. No shell XAML edit, no `if/else` routing chain, no changes to `TaskManagerModule` at all. This is the exact claim `Documentation/01-decisions-log.md` §7 made about the module contract before any second module existed to test it against — worth noting when a design decision actually gets validated by evidence, not just by having sounded reasonable at the time.

## Why Notes gets its own database file, not a shared one

`NotesDbContext` and `TaskManagerDbContext` point at two separate SQLite files (`notes.db`, `vantageflow.db`), each with its own EF Core migrations history. Two DbContexts *could* share one SQLite file, but by default they'd collide on the same `__EFMigrationsHistory` table name — avoidable with per-context configuration, but a per-module database file sidesteps the collision entirely and more directly embodies "a module could ship or version independently" from `Documentation/02-architecture-and-testing-strategy.md` §7/§10.

Generating the second migration needed one extra flag, since the project now has two `DbContext` types:

```
dotnet ef migrations add InitialCreate --context NotesDbContext --project src/VantageFlow.Core/VantageFlow.Core.csproj --startup-project src/VantageFlow/VantageFlow.csproj -o Migrations/Notes
```

`--context` disambiguates which `DbContext` to target; `-o Migrations/Notes` keeps its migration files out of `TaskManagerDbContext`'s folder — without it, `dotnet ef` would still work (each context's own migrations only ever apply to itself), but both sets of files would land in the same folder, harder to tell apart at a glance.

## What Notes deliberately doesn't have

No Requester/Project/Source-style reusable pickers, no Recipient, no edit/complete flow — none of `Documentation/Walkthroughs/05`–`08`'s richer patterns. That's intentional: this module exists to prove the *shell/composition/persistence* architecture generalizes, not to prove every TaskManager feature is worth repeating for every module. A future module that genuinely needs Person-style reusable entities can adopt that pattern then; Notes didn't need it, so it doesn't have it.
