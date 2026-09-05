namespace VantageFlow.Core;

/// <summary>
/// Lets a ViewModel navigate without referencing a concrete Page, Frame, or Window.
/// </summary>
public interface INavigationService
{
    bool CanGoBack { get; }

    void Navigate(Type pageType);

    void GoBack();
}
