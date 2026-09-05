using CommunityToolkit.Mvvm.ComponentModel;

namespace VantageFlow.Core;

public sealed partial class ShellViewModel : ObservableObject
{
    public IReadOnlyList<NavigationItem> NavigationItems { get; }

    public ShellViewModel(IReadOnlyList<IAppModule> modules)
    {
        NavigationItems = modules.SelectMany(m => m.GetNavigationItems()).ToList();
    }
}
