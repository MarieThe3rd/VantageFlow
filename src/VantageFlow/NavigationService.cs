using Microsoft.UI.Xaml.Controls;
using VantageFlow.Core;

namespace VantageFlow;

/// <summary>
/// Wraps the shell's content Frame. ShellPage calls <see cref="Initialize"/> once, after the
/// Frame exists; everything else (ViewModels included) depends only on <see cref="INavigationService"/>.
/// </summary>
public sealed class NavigationService : INavigationService
{
    private Frame? _frame;

    public bool CanGoBack => _frame?.CanGoBack ?? false;

    public void Initialize(Frame frame) => _frame = frame;

    public void Navigate(Type pageType)
    {
        if (_frame is null)
            throw new InvalidOperationException($"{nameof(NavigationService)} has not been initialized with a Frame.");

        _frame.Navigate(pageType);
    }

    public void GoBack()
    {
        if (_frame?.CanGoBack == true)
            _frame.GoBack();
    }
}
