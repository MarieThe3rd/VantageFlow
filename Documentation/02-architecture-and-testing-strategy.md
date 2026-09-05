# Architecture & Testing Strategy

The concrete shape behind the decisions in `01-decisions-log.md`. This is a living document — refine it as the app grows, but keep changes deliberate, not accidental drift.

Sections 1–5 are checked against Microsoft's own current guidance for this exact scenario — [Architecture patterns for WinUI 3 desktop apps](https://learn.microsoft.com/windows/apps/develop/architecture-patterns) and the [Packaging overview](https://learn.microsoft.com/windows/apps/package-and-deploy/packaging/) — not just the reference-app analysis. Where it differs from what we'd already planned, that's called out explicitly.

## 1. Packaging model: packaged (MSIX)

Decided (`01-decisions-log.md` §12) — per Microsoft's current default guidance: **"Building a new WinUI 3 app? You're already packaged by default. For most WinUI 3 apps, MSIX (via Store or direct download) is the better path."** No reason surfaced strong enough to deviate; if anything the task manager's plausible need for reminders that fire while the app isn't running (background tasks need package identity) points toward packaged, not just fails to argue against it.

What this buys, concretely: background tasks, push notifications, manifest-based file/protocol associations, and `ApplicationData.Current.LocalSettings` for settings storage — all of which require package identity and none of which an unpackaged app gets. The reference app went unpackaged specifically to support winget/Chocolatey/Scoop distribution outside the Store; that's a real reason for that app, not a default worth copying without the same reason here. If GitHub-release direct-download distribution is wanted later without giving up package identity, "packaged with external location" is the middle-ground path — a decision for the packaging/release setup, not the app architecture.

## 2. Composition root: DI container from the start

Add `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Hosting` — Microsoft's own architecture guidance uses this exact pair, built on `Host.CreateDefaultBuilder()` in `App.xaml.cs`:

```csharp
public partial class App : Application
{
    public IHost Host { get; }

    public static T GetService<T>() where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
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
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<MainPage>();
                // each module's RegisterServices (§3) adds to this same collection
            })
            .Build();
    }
}
```

Pages resolve their ViewModel via `App.GetService<T>()` in their constructor (not `OnNavigatedTo`, unless the page is cached and needs a fresh instance per navigation).

- Every ViewModel and service takes its dependencies through its constructor.
- Every service is `IFooService` + `FooService`, defined together, from the moment it's created — not retrofitted later.
- **Service lifetimes** (from the same guidance): `AddSingleton` for navigation and app-wide state/caches; `AddScoped` for per-window/per-dialog contexts; `AddTransient` for ViewModels and stateless services — register ViewModels as Transient so each navigation gets a fresh instance.

## 3. The module contract

```csharp
public interface IAppModule
{
    void RegisterServices(IServiceCollection services);
    IEnumerable<NavigationItem> GetNavigationItems(); // icon, label, page type
    Task StartAsync(IServiceProvider services);       // background work, if any
    Task StopAsync(IServiceProvider services);        // cleanup, if any
}
```

Composition root:

```csharp
foreach (var module in modules) module.RegisterServices(services);
// ... build provider ...
foreach (var module in modules) await module.StartAsync(provider);
// shell: foreach module, add its nav items
// on exit: foreach module (reverse order), await module.StopAsync(provider);
```

Adding a module is: write a class implementing `IAppModule`, add it to the module list. No editing shell XAML, no editing a launch `if/else` chain, no editing the exit handler.

## 4. Shell = navigation only

One page (`ShellPage`) owns the `NavigationView` + content `Frame` and nothing else. Every feature — including the task manager, which is just the first module — is its own page navigated into that `Frame`. The shell builds its nav items from the module registry (§3), never a hardcoded list.

Page template (non-negotiable shape for every page in every module): constructor takes/creates its ViewModel; `OnNavigatedTo`/`OnNavigatedFrom` (or `Loaded`/`Unloaded` if cached) do lifecycle only; every handler is a one-line forward to the ViewModel or a `[RelayCommand]` binding; the only code-behind logic allowed is genuinely visual (custom drawing/animation) — never business logic, never a direct service call.

## 5. MVVM and layering

- **CommunityToolkit.Mvvm**: `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`.
- Inject an `INavigationService` (even a thin wrapper over `Frame.Navigate`) into any ViewModel that needs to navigate — never reach for a concrete Page or a static singleton to do it. (This is the exact mistake the reference-app analysis found — a ViewModel reaching into a concrete Page's static singleton to navigate.)

Microsoft's own guidance frames this as a strict layered dependency direction, worth keeping as a standing rule for every module:

```
┌─────────────────────────────┐
│ Views (XAML + code-behind)  │  ← UI layer, no business logic
├─────────────────────────────┤
│ ViewModels (MVVM Toolkit)   │  ← Presentation logic, commands
├─────────────────────────────┤
│ Services / Use Cases        │  ← Business rules, orchestration
├─────────────────────────────┤
│ Repositories / Data         │  ← Data access, API clients, caching
└─────────────────────────────┘
```

- Each layer depends only on the layer directly below it.
- ViewModels never reference UI types (`Page`, `Window`, `ContentDialog`).
- Services define interfaces; implementations live in the data layer.
- All cross-layer dependencies go through the DI container — never a direct `new`.

## 6. Dialogs: one shape, everywhere

A `ContentDialog` subclass. Inputs through the constructor, output through a `Result` property, its own validation via `args.Cancel`, its own localization/theme. Never define a `ContentDialog` inline inside a page's XAML. This is zero-shared-infrastructure — any module can add a dialog this way needing nothing from the host page but a `XamlRoot`.

## 7. Services: narrow, split by responsibility before they merge

For each module, decide up front which of "CRUD against an external system," "domain-model mapping," "history/diagnostics," and "persistence" are separate classes with separate interfaces. Don't let one class accrete all of them. Watch for copy-pasted "almost-identical" services — that's a sign to extract one shared, parameterized abstraction instead of duplicating a file.

Default to owning a real per-module data store (SQLite via `Microsoft.Data.Sqlite` or EF Core, or per-module structured files) rather than splicing structured data into a field an external API wasn't designed to hold it — only reach for that kind of workaround if a future module genuinely has to interoperate with an equally rigid external system.

## 8. Settings: per-module shape from day one

Each module owns its own settings section (a small POCO of its own). A small settings-composition service aggregates them for persistence. Keep runtime-only state (e.g., "is a snooze currently active") structurally separate from durable user preferences from the start.

Per §1 (packaged/MSIX), storage is `ApplicationData.Current.LocalSettings` for simple key-value pairs. This is one `Core` service every module's settings section plugs into — not something each module reimplements.

Static app *configuration* (API endpoints, feature flags — values that don't change per-user) is a different concern from user *settings* — if that's ever needed, `Microsoft.Extensions.Configuration` + `appsettings.json` + `IOptions<T>` is the standard pattern, bound in the same `ConfigureServices` call as everything else.

## 9. Versioning and migration

Worth deciding the shape of this early, even before it's needed, since retrofitting a migration story onto live user data later is painful:

- **Local data schema**: version it, and run a migration step at startup that upgrades from any previous version to current (a `DatabaseMigrator`-style class with a switch on stored schema version is the standard shape). **Implemented** for the TaskManager module: real EF Core Migrations (not `EnsureCreated`, which can't evolve a schema), applied via `Database.MigrateAsync()` in `TaskManagerModule.StartAsync` on every launch — see `01-decisions-log.md` §18 and `Documentation/Walkthroughs/06`.
- **Settings schema**: same idea — store a version alongside the settings, transform old shapes forward on load. Not yet implemented (no settings exist yet).
- Each module's persistence is a natural place to own its own schema version, consistent with §7/§8's "each module owns its own store/settings" split.

## 10. Actual folder/project structure

Revised from the original single-project sketch after a real test failure forced the issue — see `01-decisions-log.md` §16 for why: WinUI app projects carry a deployment auto-initializer that throws outside a packaged app process, so anything that needs to be unit-testable (Models, ViewModels, the module/navigation contracts) has to live outside the WinUI head project entirely, in a WinUI Class Library with no `Microsoft.WindowsAppSDK` package reference. Three projects, not one:

```
src/
  VantageFlow/                     — WinUI head project (packaged/MSIX)
    App.xaml(.cs)                  — composition root
    MainWindow.xaml(.cs)           — OS window: title bar + root Frame only
    ShellPage.xaml(.cs)            — navigation shell; owns NavigationView + content Frame
    NavigationService.cs           — concrete INavigationService, wraps the real Frame
    Modules/
      TaskManager/
        TaskManagerModule.cs       — IAppModule impl: registers services, contributes nav items
        Views/                    — Pages + this module's Dialogs/
  VantageFlow.Core/                — WinUI Class Library (UseWinUI=true, no WindowsAppSDK package)
    Core/                         — IAppModule, INavigationService, NavigationItem, NavigationIcon
    Modules/
      TaskManager/
        Models/
        ViewModels/
        Data/                     — DbContext + EF Core model configuration (§9 persistence)
        Services/                 — IFooRepository + EF Core–backed implementation pairs
    Migrations/                   — EF Core migrations (one folder per DbContext, if more than one)
    ShellViewModel.cs
  VantageFlow.Tests/               — references VantageFlow.Core only, never the app project
    Modules/TaskManager/           — mirrors VantageFlow.Core's module folders
```

The rule that keeps this working: anything in `VantageFlow.Core` — transitively, everything it references — must compile without a live WinUI type (`Frame`, `Symbol`, `Page`, ...). `NavigationIcon` (a plain enum) exists specifically so `IAppModule`/`NavigationItem` can stay in Core while the real WinUI `Symbol` mapping happens only in `ShellPage`. When a new module needs a Service (not just Models/ViewModels), the same split applies: the interface and any WinUI-free implementation go in Core; only a concrete piece that must touch a live UI type moves to the head project.

Splitting into further assemblies (one per module, instead of folders within these three) is a later decision, only if a module needs to ship or version independently.

## 11. Testing strategy

Three tiers, thickest at the bottom. The goal throughout: **test behavior, not implementation** — assert on resulting state/output through a public interface, not on which internal method got called.

### Tier 1 — Logic tests (bulk of the suite)

Every Service and ViewModel tested through its public interface/constructor injection. Fakes only at true external boundaries (the OS, disk, network) — never mock your own internal collaborators just to verify an interaction happened. Pure functions (classification, formatting, parsing) as small, dependency-free static classes are the cheapest, highest-value tests to write — reach for this shape whenever logic doesn't need external state.

### Tier 2 — Module integration tests

A module's real services wired together (no fakes except the outermost boundary), driven through its ViewModel. No UI involved — still fast, but catches wiring bugs pure isolated unit tests miss.

### Tier 3 — UI/E2E tests (thin, top of the pyramid)

A small number of true through-the-UI tests, reserved for golden paths only (e.g., "create a task → verify it's scheduled and visible," "snooze → verify state") — things only the real running UI can catch (layout, focus, visible state), not business-logic edge cases, which belong in Tier 1.

**Tool: FlaUI**, not Playwright. Verified against Microsoft's current testing docs: Playwright's Windows-app support is scoped to WebView2 content only — it cannot drive native XAML controls (`NavigationView`, `ContentDialog`, buttons, etc.), because those aren't a browser context. Native UI Automation is the only path for WinUI 3. Microsoft's current guidance recommends Appium + the Windows driver plugin (the actively maintained successor to the now-inactive WinAppDriver), but that pulls in a Node.js/Appium server process — worthwhile for polyglot cross-platform test harnesses, unnecessary overhead for a single C#/WinUI3 app. FlaUI is a plain, MIT-licensed .NET library over the same UI Automation tree, called directly from xUnit with no separate server to manage — the simpler fit here.

WinUI 3 controls expose UI Automation peers natively, so this is a well-trodden path, not an experimental one.
