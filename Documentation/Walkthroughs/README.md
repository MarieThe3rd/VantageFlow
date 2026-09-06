# Walkthroughs

This folder is a step-by-step companion to VantageFlow's own code, written as it's built. VantageFlow is deliberately being used as the vehicle for learning WinUI 3 desktop development (see `CLAUDE.md`), the same way past projects served as the vehicle for learning Blazor and React+TypeScript — so this isn't a general WinUI 3 tutorial, it's a walkthrough of *this codebase specifically*, explaining both what each piece does and which WinUI 3 / XAML / MVVM practice it demonstrates.

Written bridging from strong backend .NET/C# experience — no prior desktop-UI or XAML background assumed. Concepts that have a web/Blazor analogue call it out (e.g., `{x:Bind}` vs. Razor's `@bind`); concepts that don't (native UI Automation, `ContentDialog`, package identity) are explained from first principles.

## Reading order

Numbered in the order the corresponding code was actually built, not by folder or topic — read top to bottom to follow the app's construction chronologically.

1. [01-winui3-project-anatomy.md](01-winui3-project-anatomy.md) — the packaged-app template: manifest, window, page, assets
2. [02-composition-root-and-di.md](02-composition-root-and-di.md) — `App.xaml.cs`, the DI container, `App.GetService<T>()`
3. [03-navigation-shell-and-modules.md](03-navigation-shell-and-modules.md) — `NavigationView`, `IAppModule`, how a module plugs in
4. [04-why-a-separate-class-library-for-tests.md](04-why-a-separate-class-library-for-tests.md) — the real test failure that forced the three-project split
5. [05-add-task-dialogs-and-converters.md](05-add-task-dialogs-and-converters.md) — `ContentDialog`, `IValueConverter`, and a `required`-properties gotcha in generated XAML metadata
6. [06-sqlite-persistence-with-ef-core.md](06-sqlite-persistence-with-ef-core.md) — `IDbContextFactory` for desktop apps, shadow FK properties, the disconnected-entity gotcha, and running EF migrations against a class-library project
7. [07-recipient-project-source-fields.md](07-recipient-project-source-fields.md) — scaling the reusable-picker pattern, conditional fields via `SelectionChanged`, and a `DateOnly`/`CalendarDatePicker` type gap
8. [08-editing-completing-and-full-display.md](08-editing-completing-and-full-display.md) — binding a converter to the whole object, one dialog serving both create and edit, forcing a `ListView` to notice an in-place mutation, and updating a disconnected entity graph safely
9. [09-a-second-module-proves-the-pattern.md](09-a-second-module-proves-the-pattern.md) — adding the Notes module: one line in the composition root, a separate per-module database file, and `dotnet ef`'s `--context` flag once a project has more than one `DbContext`

More entries get added as each meaningful chunk of code lands (see `CLAUDE.md`'s "Learning companion" convention).

## Format for each entry

- **What was built** — the feature/file(s), in plain terms.
- **The code, annotated** — the actual snippet with inline explanation, not a paraphrase.
- **The WinUI 3 practice it demonstrates** — the pattern/API/idiom, and why it's the recommended way (cite `02-architecture-and-testing-strategy.md` or Microsoft's docs where relevant).
- **How it compares** — a one- or two-line bridge from prior experience, when there's a natural analogue (Blazor, ASP.NET Core, or plain C#/.NET) — skip this if there isn't one, don't force it.
