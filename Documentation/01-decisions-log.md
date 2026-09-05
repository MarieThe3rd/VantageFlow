# Decisions Log

Running log of decisions made in conversation, in the order made, with the reasoning behind each. One-line summaries live in `CLAUDE.md`; the "why" lives here so it isn't lost.

## 1. Build fresh, don't fork/refactor an existing app

The starting point for this project was analyzing a forked WinUI 3 app (a Windows Task Scheduler GUI) as a reference for what a well-built vs. poorly-built WinUI 3 app looks like. That analysis is not carried into this repo verbatim, but its conclusions directly shaped the decisions below.

The reference app had no DI container anywhere (every service a `public static class` global singleton), a 1734-line god-service wrapping the Task Scheduler API, and a 3294-line god-page fusing its navigation shell with its main feature — none of it protected by characterization tests. Retrofitting that into a well-architected, modular app would cost roughly as much as building it right from the start, with the added risk of breaking a working app along the way. Decision: build fresh, keep the reference app's *patterns* (documented below) rather than its code.

The reference app is MIT-licensed, so copying a specific small utility from it later (e.g., a debugged edge case) would be legally fine with attribution — but that's a case-by-case call, not a starting position.

## 2. App name: VantageFlow

Considered: RadarFlow, Vantage, Sightline, Wayfinder, Beacon, Compass-style names. Landed on **VantageFlow** — "vantage" for a single elevated view across everything being tracked regardless of module, "flow" for it staying smooth as modules are added. Confirmed available under the `MarieThe3rd` GitHub account before creating the repo.

## 3. Repo setup: public, owner-gated merges

- Created as `github.com/MarieThe3rd/VantageFlow`, public visibility (deliberate choice — public from day one).
- Branch protection on `main`: requires a PR to merge, gated on approval from `.github/CODEOWNERS` (`* @MarieThe3rd` — the only code owner). `enforce_admins: false`, so the owner can still push directly or merge without a review; anyone else must open a PR and get the owner's approval.
- Note: GitHub's literal "restrict who can push, by username" branch protection field only works on organization-owned repos, not personal accounts — attempted first, rejected by the API with `"Only organization repositories can have users and team restrictions"`. CODEOWNERS + required review is the personal-repo equivalent and achieves the same real outcome (only the owner gates what lands on `main`).

## 4. Platform target: x64 only, ARM64 deferred

The reference app ships both x64 and ARM64 (for Copilot+ PC / Snapdragon Windows laptops). x64 covers the vast majority of Windows installs, and an x64 app still runs on ARM64 Windows via emulation. Publishing a second architecture is a `dotnet publish -p:Platform=ARM64` + one CI matrix row — a build-time setting, not something baked into the app's design. Decision: start x64-only, add ARM64 later only if actual ARM users show up.

## 5. Architecture: modular monolith, module = vertical slice

One deployable app, split into independent modules (task manager is module #1). Each module owns its full stack — Models, Services, ViewModels, Views, Dialogs — rather than the reference app's horizontal layering (one flat `Services/` folder shared by everything). Modules interact with the rest of the app only through a small `IAppModule` contract (see `02-architecture-and-testing-strategy.md`), never by reaching into another module's internals.

This is deliberately *module-level* vertical slicing, not the finer-grained per-use-case slicing (a `CreateTask` command/handler pair, MediatR-style) that "Vertical Slice Architecture" usually means in web-API contexts. Per-use-case slicing is a later, optional refinement inside a module if one grows large enough to need it — not the starting granularity.

## 6. Real DI container + interface-per-service from day one

The single biggest fix relative to the reference app, which had zero DI anywhere. Every service gets an interface the moment it's created (not retrofitted under test pressure later), and every ViewModel/service receives its dependencies through its constructor — never a bare `new SomeService()`, never a static-class call from a ViewModel.

## 7. MVVM toolkit: CommunityToolkit.Mvvm

Chosen over hand-rolling `INotifyPropertyChanged` (which the reference app did everywhere, consistently, but without a toolkit — pure boilerplate for the same result). `[ObservableProperty]`/`[RelayCommand]` source generators get real `ICommand` bindings instead of code-behind Click handlers calling ViewModel methods directly. An injected navigation service goes with this, specifically to avoid the one concrete MVVM-direction violation found in the reference app (a ViewModel reaching into a concrete Page's static singleton to navigate).

## 8. Testing philosophy: behavior over implementation, three tiers

- **Logic tests (bulk of the suite):** every Service/ViewModel tested through its public interface, fakes only at true external boundaries (OS APIs, disk, network) — never mocking your own internal collaborators just to assert a method got called.
- **Module integration tests:** a module's real services wired together (no fakes except the outermost boundary), driven through its ViewModel — no UI involved.
- **UI/E2E tests (thin, top of the pyramid):** a handful of true through-the-UI tests for golden paths only.

## 9. UI test tool: FlaUI, not Playwright

Verified against Microsoft's current WinUI/WindowsAppSDK testing docs before deciding (not from memory): Playwright's support for Windows apps is scoped specifically to **WebView2** content — it cannot drive native XAML controls at all, because they aren't a browser context. Native UI Automation is the only path for WinUI 3, and Microsoft's current recommendation there is Appium + the Windows driver plugin (WinAppDriver itself is no longer actively developed). For a single-platform C# app, **FlaUI** (a plain MIT-licensed .NET library over UI Automation, no Appium/Node server to run) is the simpler, more idiomatic fit — call it directly from xUnit like any other test.

## 10. Open decisions

- **License:** not yet chosen.
- **Persistence:** not yet decided how the task manager module stores data (reference app spliced its metadata into the OS Task Scheduler's free-text field as a workaround for that specific external API — this app should default to owning a real data store per module, e.g. SQLite, rather than needing a similar trick).
- **Packaging model (MSIX vs. unpackaged):** not yet decided — see `02-architecture-and-testing-strategy.md` §1. Matters because it gates background-task support, push notifications, and settings-storage API, so it needs deciding before the composition root is written.

## 11. Core architecture checked against Microsoft's own guidance

Before locking in §5–9 above, searched and fetched Microsoft's current official docs (`Architecture patterns for WinUI 3 desktop apps`, `Packaging overview`) rather than relying on training-data recall. Result: the DI/MVVM/layering plan already matched almost exactly (same `Microsoft.Extensions.DependencyInjection` + `Hosting` pair, same `App.GetService<T>()` composition-root shape, same "ViewModels never reference UI types" rule). One gap surfaced that wasn't previously considered: **packaging model** (added to Open Decisions above) — Microsoft's current default guidance is that a new WinUI 3 app should stay packaged (MSIX) unless there's a specific reason not to, which cuts against the reference app's unpackaged/winget-style distribution choice. Full detail and the questions that decide it are in `02-architecture-and-testing-strategy.md` §1.
