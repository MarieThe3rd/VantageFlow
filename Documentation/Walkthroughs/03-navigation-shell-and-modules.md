# Navigation Shell and the Module Contract

**What was built**: `ShellPage` (a `NavigationView` driving a content `Frame`), `IAppModule`, and `TaskManagerModule` — the first module plugging into it.

## The code, annotated

`IAppModule` (in `VantageFlow.Core`) is deliberately small:

```csharp
public interface IAppModule
{
    void RegisterServices(IServiceCollection services);
    IEnumerable<NavigationItem> GetNavigationItems();
    Task StartAsync(IServiceProvider services);
    Task StopAsync(IServiceProvider services);
}
```

`TaskManagerModule` (in the `VantageFlow` head project, since it references a real `Page` type) implements it:

```csharp
public sealed class TaskManagerModule : IAppModule
{
    public void RegisterServices(IServiceCollection services) =>
        services.AddTransient<TasksViewModel>();

    public IEnumerable<NavigationItem> GetNavigationItems()
    {
        yield return new NavigationItem("Tasks", NavigationIcon.List, typeof(TasksPage));
    }

    public Task StartAsync(IServiceProvider services) => Task.CompletedTask;
    public Task StopAsync(IServiceProvider services) => Task.CompletedTask;
}
```

`ShellPage` never knows `TaskManagerModule` or `TasksPage` exist — it only knows `IReadOnlyList<IAppModule>` (injected) and builds real `NavigationViewItem`s from whatever `NavigationItem`s the modules hand back:

```csharp
foreach (var item in _viewModel.NavigationItems)
{
    Nav.MenuItems.Add(new NavigationViewItem
    {
        Content = item.Label,
        Icon = new SymbolIcon(ToSymbol(item.Icon)),
        Tag = item.PageType,      // stashed here, read back in Nav_ItemInvoked
    });
}
```

`NavigationView.ItemInvoked` fires when the user clicks a menu entry; the handler reads the `Type` back off `Tag` and hands it to `NavigationService.Navigate(pageType)`, which calls the real `Frame.Navigate(pageType)` underneath.

## The WinUI 3 practice this demonstrates

`NavigationView` is WinUI's standard "hamburger menu + content area" control — the same shape as almost every Windows 11 Settings-style app. Building its `MenuItems` in code-behind from a bound ViewModel property (rather than XAML `MenuItemsSource` + `DataTemplate`) is a legitimate, common shortcut for this exact case: constructing real UI elements (`NavigationViewItem`) *is* view-construction, not business logic, so it's fine for it to live in `ShellPage`'s code-behind — the rule "ViewModels never reference UI types" is about ViewModels, not about a page building its own controls.

`Tag` as a stash for arbitrary non-visual data (here, a `Type`) on a UI element is an old, very common WinUI/WPF pattern predating strongly-typed alternatives — worth knowing it exists, even though you'd reach for a proper binding in more complex cases.

## Why the icon isn't just `Symbol`

`NavigationItem.Icon` is a plain `NavigationIcon` enum (defined in `VantageFlow.Core`), not WinUI's own `Symbol` enum — see `Documentation/01-decisions-log.md` §16. `Symbol` lives in `Microsoft.UI.Xaml.Controls`, a real WinUI type, and `VantageFlow.Core` has to stay free of those to remain unit-testable. `ShellPage.ToSymbol(...)` is the one place that translation happens — a small, deliberate seam between "what a module says it needs" and "how the shell actually renders it."
