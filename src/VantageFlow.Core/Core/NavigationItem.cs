namespace VantageFlow.Core;

/// <summary>
/// One entry a module contributes to the shell's navigation menu.
/// </summary>
public sealed record NavigationItem(string Label, NavigationIcon Icon, Type PageType);
