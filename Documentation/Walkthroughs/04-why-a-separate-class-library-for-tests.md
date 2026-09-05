# Why VantageFlow.Core Exists: a Real Test Failure, Not a Guess

**What was built**: the project split into three — `VantageFlow` (WinUI head), `VantageFlow.Core` (a *WinUI Class Library*), `VantageFlow.Tests` — after the very first test written against the original two-project layout failed in a way that had nothing to do with the test's own logic.

## What actually happened

The first test wasn't complicated:

```csharp
[Fact]
public void NewTask_DefaultsToObligation()
{
    var task = new TaskItem { Title = "Anything" };
    Assert.Equal(Commitment.Obligation, task.Commitment);
}
```

`TaskItem` is a plain C# class with no WinUI dependency at all. But at the time, `VantageFlow.Tests` referenced the `VantageFlow` WinUI app project directly (the only place `TaskItem` lived), and running the test threw this instead of a normal pass/fail:

```
System.TypeInitializationException: The type initializer for '<Module>' threw an exception.
 ---> System.Runtime.InteropServices.COMException: Class not registered (0x80040154)
    at Microsoft.Windows.ApplicationModel.WindowsAppRuntime.DeploymentManagerCS.AutoInitialize...
```

Nothing about `TaskItem` or the assertion caused this. **Just loading the app assembly at all** was enough — the `Microsoft.WindowsAppSDK` NuGet package that a packaged WinUI app references injects a module initializer that tries to bootstrap the Windows App Runtime deployment on first use, and that bootstrap fails outside a real packaged app process (which a plain `dotnet test` run is not).

## What confirmed the fix

Before restructuring anything, this was checked against Microsoft's own current docs (`Test WinUI apps built with the Windows App SDK`) rather than guessed at. Their guidance is direct: **"unit test projects can't directly reference WinUI app projects."** Their prescribed fix is to move anything that needs testing — ViewModels, Models, services — into a separate **WinUI Class Library** project, and have both the app and the test project reference *that* instead.

## The fix, and the one wrinkle it created

`VantageFlow.Core` is that library: `UseWinUI=true` (so it can compile against `Microsoft.UI.Xaml` types if it ever truly needs to) but critically **no `Microsoft.WindowsAppSDK` package reference** — that's the package carrying the deployment auto-initializer, so a library that doesn't reference it doesn't inherit the problem.

The wrinkle: the class library template defaults to `WinUISDKReferences=false`, which means it *can't actually see* `Microsoft.UI.Xaml.Controls` types like `Frame` or `Symbol` at all — not "can reference them but shouldn't," but a genuine compile error (`CS0234`). That forced a second, smaller decision: `NavigationItem`'s icon became a plain `NavigationIcon` enum defined in Core instead of WinUI's `Symbol`, and the concrete `NavigationService` (which wraps a real `Frame`) moved back into the `VantageFlow` head project, alongside `ShellPage` — the only place that needs to translate between the two.

## The takeaway

"Can I unit test this?" turned out to depend on which *project* a type lives in, not just whether the type itself has business logic. Any Model or ViewModel meant to be tested has to live somewhere with zero path back to `Microsoft.WindowsAppSDK` — which is exactly why `VantageFlow.Core` exists as a third project instead of two.
