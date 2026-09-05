# Filling Out the Task Domain: Recipient, Project, Source

**What was built**: `AddProjectDialog` and `AddSourceDialog` (same shape as `AddPersonDialog`), and `AddTaskDialog` extended with Recipient/Project/Source pickers plus conditional Ticket Number/Link fields — every field from `CONTEXT.md`'s `Task` is now capturable.

## Scaling a pattern instead of repeating its reasoning

`ProjectRepository`/`SourceRepository`, `AddProjectDialog`/`AddSourceDialog`, and "New Project"/"New Source" buttons on `TasksPage` are structurally identical to the Person versions from `Documentation/Walkthroughs/05` and `06` — same reusable-entity-not-free-text reasoning, same repository shape, same dialog shape. Worth noticing when a pattern is genuinely repeating (add the fourth one without re-deriving why) versus when something is actually different enough to need new thought (the next two sections).

## Conditional fields: `SelectionChanged`, not a converter

Ticket Number/Link only make sense when the selected Source `IsTicket`. This is different from `Documentation/Walkthroughs/05`'s converters (`RequesterToSummaryConverter` etc.) — those reshape a *bound* value for display; this needs to react to a *user action* (picking a different Source) and change which controls are even visible. A binding can't easily watch "the currently selected item of a different control" without extra machinery, so this uses the direct, simpler tool for the job:

```csharp
private void SourceCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    var isTicket = SourceCombo.SelectedItem is Source { IsTicket: true };
    TicketFieldsPanel.Visibility = isTicket ? Visibility.Visible : Visibility.Collapsed;
}
```

`Source { IsTicket: true }` is a recursive pattern — a type check (`is Source`) and a property check (`IsTicket == true`) in one expression, short-circuiting cleanly to `false` if `SelectedItem` is `null` or not a `Source` at all (nothing selected yet). This is plain code-behind, not a converter, because it's driving *which controls exist in the visual tree right now*, not reshaping one value for one binding — a legitimate view concern, same category as building `NavigationViewItem`s in `Documentation/Walkthroughs/03`.

## `CalendarDatePicker` and the type gap between XAML and the domain

`Project.TargetDate` is a `DateOnly?` (deliberately — a target date has no meaningful time-of-day). WinUI's date-picking controls predate `DateOnly` (a C# 10 addition) and work in `DateTimeOffset?`:

```csharp
TargetDate = TargetDatePicker.Date is { } date ? DateOnly.FromDateTime(date.Date) : null,
```

`.Date` on the `DateTimeOffset` strips the time-of-day component (defensively — `CalendarDatePicker` shouldn't set one, but nothing enforces that), then `DateOnly.FromDateTime` converts. Small, but a real seam worth knowing about any time a domain model uses a modern .NET type a XAML control's API predates.

## What every dialog in this app now has in common

Four dialogs in (`AddPersonDialog`, `AddTaskDialog`, `AddProjectDialog`, `AddSourceDialog`), the shape hasn't bent once: constructor-in, `Result`-property-out, validate in `PrimaryButtonClick` with `args.Cancel`. That consistency is the actual payoff of writing it down as a rule in `Documentation/02-architecture-and-testing-strategy.md` §6 before the second dialog ever got built, rather than discovering the shape by accretion.
