# VantageFlow

A modular WinUI 3 desktop app. First module is a task manager; the architecture exists specifically so future modules (notes, habits, whatever comes next) plug in without editing shared code. Currently in planning/scaffolding — no application code yet.

This file is the always-loaded index. Keep it short: one line per decision, with the reasoning and full detail pushed to `Documentation/`. When a new stable decision gets made in conversation, add its one-liner here and its rationale there — don't let this file grow into the reasoning itself.

## Locked decisions

- **Not a fork/refactor** of any reference app — built fresh. See `Documentation/01-decisions-log.md` #1 for why.
- **Repo:** github.com/MarieThe3rd/VantageFlow, public. `main` requires a PR + CODEOWNERS approval from `@MarieThe3rd` to merge; the owner can still push directly.
- **Platform:** x64 only for now — ARM64 deferred (adding it later is a publish/CI setting, not an architecture change).
- **Packaging:** packaged (MSIX), per Microsoft's current default guidance — gets background tasks, push notifications, and `ApplicationData.Current.LocalSettings` for free.
- **Architecture:** modular monolith, organized as vertical slices per module — not fine-grained per-use-case slicing. Real DI container + interface-per-service from day one. Full contract and folder layout in `Documentation/02-architecture-and-testing-strategy.md`.
- **MVVM:** CommunityToolkit.Mvvm + an injected navigation service; one dialog shape everywhere (constructor-in, `Result`-property-out).
- **Testing:** behavior over implementation — public interfaces and fakes at true external boundaries only, never interaction-testing your own collaborators. Three tiers: logic unit tests (bulk), module integration tests, thin top-layer UI tests for golden paths. UI-automation tool is **FlaUI**, not Playwright (Playwright only drives WebView2 content, not native XAML). Full rationale in `Documentation/02-architecture-and-testing-strategy.md`.
- **License:** not yet decided — open item.

## Reference material

- `Documentation/01-decisions-log.md` — every decision above, with the reasoning behind it, in the order it was made.
- `Documentation/02-architecture-and-testing-strategy.md` — the module contract (`IAppModule`), starting folder structure, and the full testing-strategy writeup.
- `Documentation/Walkthroughs/` — a step-by-step, code-level companion explaining what was built and which WinUI 3 practice it demonstrates.

## Learning companion (standing convention)

This project is an explicit vehicle for learning WinUI 3 desktop development, bridging from strong backend .NET/C# experience with no assumed XAML/desktop-UI background. **After adding any meaningful chunk of application code, add a matching entry to `Documentation/Walkthroughs/`** (format described in that folder's `README.md`) before moving on — don't wait to be asked. Skip this convention only for pure docs/config changes with no new code pattern to explain.
