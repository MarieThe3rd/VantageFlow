# Architecture & Testing Strategy

The concrete shape behind the decisions in `01-decisions-log.md`. This is a living document — refine it as the app grows, but keep changes deliberate, not accidental drift.

## 1. Composition root: DI container from the start

Add `Microsoft.Extensions.DependencyInjection` (and `Microsoft.Extensions.Hosting` if hosted-service-style start/stop is useful). Both work fine in an unpackaged WinAppSDK app.

- Build the container once, in the composition root (`App.xaml.cs`'s constructor / `OnLaunched`), store the `IServiceProvider` on `App`.
- Every ViewModel and service takes its dependencies through its constructor.
- Every service is `IFooService` + `FooService`, defined together, from the moment it's created — not retrofitted later.

## 2. The module contract

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

## 3. Shell = navigation only

One page (`ShellPage`) owns the `NavigationView` + content `Frame` and nothing else. Every feature — including the task manager, which is just the first module — is its own page navigated into that `Frame`. The shell builds its nav items from the module registry (§2), never a hardcoded list.

Page template (non-negotiable shape for every page in every module): constructor takes/creates its ViewModel; `OnNavigatedTo`/`OnNavigatedFrom` (or `Loaded`/`Unloaded` if cached) do lifecycle only; every handler is a one-line forward to the ViewModel or a `[RelayCommand]` binding; the only code-behind logic allowed is genuinely visual (custom drawing/animation) — never business logic, never a direct service call.

## 4. MVVM

- **CommunityToolkit.Mvvm**: `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`.
- Inject an `INavigationService` (even a thin wrapper over `Frame.Navigate`) into any ViewModel that needs to navigate — never reach for a concrete Page or a static singleton to do it.

## 5. Dialogs: one shape, everywhere

A `ContentDialog` subclass. Inputs through the constructor, output through a `Result` property, its own validation via `args.Cancel`, its own localization/theme. Never define a `ContentDialog` inline inside a page's XAML. This is zero-shared-infrastructure — any module can add a dialog this way needing nothing from the host page but a `XamlRoot`.

## 6. Services: narrow, split by responsibility before they merge

For each module, decide up front which of "CRUD against an external system," "domain-model mapping," "history/diagnostics," and "persistence" are separate classes with separate interfaces. Don't let one class accrete all of them. Watch for copy-pasted "almost-identical" services — that's a sign to extract one shared, parameterized abstraction instead of duplicating a file.

Default to owning a real per-module data store (SQLite via `Microsoft.Data.Sqlite` or EF Core, or per-module structured files) rather than splicing structured data into a field an external API wasn't designed to hold it — only reach for that kind of workaround if a future module genuinely has to interoperate with an equally rigid external system.

## 7. Settings: per-module shape from day one

Each module owns its own settings section (a small POCO of its own). A small settings-composition service aggregates them for persistence (one JSON file with named sections, or one file per module). Keep runtime-only state (e.g., "is a snooze currently active") structurally separate from durable user preferences from the start.

## 8. Suggested starting folder structure

One WinUI project is enough to get modularity — splitting into multiple assemblies is a later decision, only if a module needs to ship or version independently.

```
/App                     — composition root: Program.cs, App.xaml(.cs), ShellPage, module registry/list
/Core                    — cross-cutting, used by every module: IAppModule, INavigationService,
                             base ViewModel (if not fully covered by the MVVM toolkit), shared converters
/Modules
  /TaskManager
    /Models
    /ViewModels
    /Services            — IFooService + implementation pairs
    /Views               — pages + this module's Dialogs/
  /Notes                 — same shape, once it's added
  /...
/Tests                   — one test project, folders mirroring /Modules
```

"Everything about the task manager" — and later, "everything about notes" — is one subtree, not scattered across flat top-level folders every module dumps files into.

## 9. Testing strategy

Three tiers, thickest at the bottom. The goal throughout: **test behavior, not implementation** — assert on resulting state/output through a public interface, not on which internal method got called.

### Tier 1 — Logic tests (bulk of the suite)

Every Service and ViewModel tested through its public interface/constructor injection. Fakes only at true external boundaries (the OS, disk, network) — never mock your own internal collaborators just to verify an interaction happened. Pure functions (classification, formatting, parsing) as small, dependency-free static classes are the cheapest, highest-value tests to write — reach for this shape whenever logic doesn't need external state.

### Tier 2 — Module integration tests

A module's real services wired together (no fakes except the outermost boundary), driven through its ViewModel. No UI involved — still fast, but catches wiring bugs pure isolated unit tests miss.

### Tier 3 — UI/E2E tests (thin, top of the pyramid)

A small number of true through-the-UI tests, reserved for golden paths only (e.g., "create a task → verify it's scheduled and visible," "snooze → verify state") — things only the real running UI can catch (layout, focus, visible state), not business-logic edge cases, which belong in Tier 1.

**Tool: FlaUI**, not Playwright. Verified against Microsoft's current testing docs: Playwright's Windows-app support is scoped to WebView2 content only — it cannot drive native XAML controls (`NavigationView`, `ContentDialog`, buttons, etc.), because those aren't a browser context. Native UI Automation is the only path for WinUI 3. Microsoft's current guidance recommends Appium + the Windows driver plugin (the actively maintained successor to the now-inactive WinAppDriver), but that pulls in a Node.js/Appium server process — worthwhile for polyglot cross-platform test harnesses, unnecessary overhead for a single C#/WinUI3 app. FlaUI is a plain, MIT-licensed .NET library over the same UI Automation tree, called directly from xUnit with no separate server to manage — the simpler fit here.

WinUI 3 controls expose UI Automation peers natively, so this is a well-trodden path, not an experimental one.
