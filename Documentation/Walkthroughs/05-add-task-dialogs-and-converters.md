# Adding a Task: Dialogs, Value Converters, and a `required` Gotcha

**What was built**: `AddTaskDialog` and `AddPersonDialog` (both `ContentDialog` subclasses), `TasksPage`'s "New Task"/"New Person" buttons, and two `IValueConverter`s for displaying a `TaskItem` in the list.

## The code, annotated

`TasksPage.xaml.cs` owns dialog-showing; the ViewModel only ever receives a finished result:

```csharp
private async void NewTask_Click(object sender, RoutedEventArgs e)
{
    var dialog = new AddTaskDialog(ViewModel.People) { XamlRoot = XamlRoot };
    if (await dialog.ShowAsync() == ContentDialogResult.Primary && dialog.Result is not null)
    {
        ViewModel.AddTask(dialog.Result);
    }
}
```

This is the constructor-in/`Result`-property-out dialog shape from `Documentation/02-architecture-and-testing-strategy.md` §6, applied twice. `AddTaskDialog` takes the current `People` list through its constructor (read-only, just to populate a `ComboBox`) and validates in `PrimaryButtonClick`:

```csharp
private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
{
    var title = TitleBox.Text.Trim();
    if (title.Length == 0)
    {
        args.Cancel = true;   // keeps the dialog open instead of closing with a bad result
        return;
    }
    Result = new TaskItem { Title = title, Requester = RequesterCombo.SelectedItem as Person, /* ... */ };
}
```

## Why there are two dialogs, not one with a free-text Requester field

`Documentation/01-decisions-log.md` §13 decided Requester must come from a reusable `Person` list, never typed per task. A plain `TextBox` for "Requester name" would quietly violate that decision (and "Sarah" and "sarah m." would become two different people). So `TasksPage` exposes "New Person" as its own action, and `AddTaskDialog`'s `ComboBox` only ever offers *existing* `Person` entries — if the list is empty, the Requester field is simply empty, which is a legitimate state (Requester is optional), not a broken one.

One thing tried and reverted: having "New Person" as a button *inside* `AddTaskDialog` itself (so you'd never leave the task form to add someone). WinUI only allows one `ContentDialog` open at a time per `XamlRoot` — opening a second one requires `Hide()`-ing the first, awaiting the second, then re-`ShowAsync()`-ing the first, which works but is a fragile, easy-to-get-wrong pattern for something this simple. Two sibling actions on the page is both simpler and more robust.

## Value converters: keeping display formatting out of the model

`TaskItem`/`Person` are meant to stay plain data (see `CONTEXT.md`) — no "how should this look in a list" logic on the model itself. WinUI's `IValueConverter` is exactly the seam for that:

```csharp
public sealed class RequesterToSummaryConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is Person person ? $"— asked by {person.Name}" : string.Empty;
    // ConvertBack throws NotSupportedException — this binding is one-way, so it's never called.
}
```

Registered once in `Page.Resources` and referenced from a binding: `{Binding Requester, Converter={StaticResource RequesterToSummaryConverter}}`. Unlike some XAML dialects, WinUI has no implicit bool→`Visibility` (or enum→string) coercion — a converter (or a bindable computed property) is the only way to reshape a value between the model and the view.

## The `required` + generated XAML metadata gotcha

`Person.Name` and `TaskItem.Title` were originally C# 11 `required` properties — a compile-time guarantee that no one constructs one without a name/title. The build failed anyway, in generated code:

```
XamlTypeInfo.g.cs(379,58): error CS9035: Required member 'Person.Name' must be set in the object initializer
```

`XamlTypeInfo.g.cs` is WinUI's auto-generated type-metadata provider — it exists so compiled bindings (`x:Bind`, and classic `{Binding}` in a `DataTemplate`) can look up properties without reflection at runtime. For every type reachable from a binding anywhere in the app's XAML (here, `TaskItem` and `Person`, via `{x:Bind ViewModel.Tasks}` and the `Requester` binding), it generates a parameterless activator — and a parameterless `new Person()` can never satisfy a `required` member, so the *generated* code fails to compile, not anything hand-written.

The fix: dropped `required`, kept `Name`/`Title` as plain `string` defaulting to `string.Empty`, and left real validation where it already needed to live anyway — `args.Cancel = true` in the dialogs. `required` only would have stopped *omitting* the property in an object initializer; it never would have stopped someone passing `""`, so the dialog-level check was always the actual guard.

**The takeaway**: any Model or ViewModel property that a `DataTemplate` binds to — even via classic `{Binding}`, not just `x:Bind` — needs a parameterless-constructible shape, because WinUI's generated metadata provider needs to be able to construct one. `required` and "used somewhere in a binding" don't mix.
