# WinUI 3 Project Anatomy

**What was built**: the initial `VantageFlow` project, scaffolded from Microsoft's official `winui3` template (`dotnet new winui3`), packaged (MSIX) per `Documentation/01-decisions-log.md` §12.

## The pieces, and what each one is for

- **`Package.appxmanifest`** — the thing that makes this a *packaged* app. It declares an `Identity` (name, publisher, version — this is what gives the app "package identity," the prerequisite for background tasks, push notifications, and `ApplicationData.Current.LocalSettings`), the visible tiles/logos, and `Capabilities` the app needs. An unpackaged app has none of this — no manifest, no identity, just a folder of files.
- **`App.xaml` / `App.xaml.cs`** — `Application.OnLaunched` is the actual entry point (not `Main`/`Program.cs` in the traditional console sense — WinUI generates its own). This is where the app decides what window to show.
- **`MainWindow.xaml(.cs)`** — the OS window: title bar, and a root `Frame` that displays whichever `Page` is currently navigated to. A WinUI 3 desktop app can have multiple windows; this template gives you one.
- **`ShellPage.xaml(.cs)`** (renamed from the template's `MainPage`) — the first `Page` navigated into that root `Frame`. Pages are WinUI's unit of navigation — think of it as roughly analogous to a routed component in a web SPA, except the "route" is a .NET `Type`, not a URL string (`Frame.Navigate(typeof(ShellPage))`).
- **`Assets/`** — the various logo sizes Windows needs for the Start menu tile, taskbar, splash screen, etc. — a packaging requirement, not something hand-drawn per image; Visual Studio (or the template) generates the standard set from one source image.

## Bridging from web/backend experience

If you've built a Blazor or ASP.NET Core app: `App.xaml.cs`'s `OnLaunched` plays a similar role to `Program.cs`'s `WebApplication.CreateBuilder(...).Build().Run()` — it's the one place startup wiring happens. `Frame.Navigate(typeof(Page))` is the desktop analogue of client-side routing, except there's no URL — the "address" of a page is its C# type, resolved and matched entirely at compile time.

The biggest mental shift from web development: there's no request/response cycle. The app is one long-running process; `OnLaunched` runs once, and everything after that is event-driven (button clicks, navigation, timers) for as long as the process is alive.
