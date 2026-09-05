using Microsoft.UI.Xaml.Controls;
using VantageFlow.Core;

namespace VantageFlow;

/// <summary>
/// Navigation shell: builds the menu from <see cref="ShellViewModel.NavigationItems"/> and
/// wires the content Frame into <see cref="NavigationService"/>. No feature logic lives here —
/// see the "Learning companion" convention in CLAUDE.md for why that boundary matters.
/// </summary>
public sealed partial class ShellPage : Page
{
    private readonly ShellViewModel _viewModel;

    public ShellPage()
    {
        InitializeComponent();

        _viewModel = App.GetService<ShellViewModel>();
        App.GetService<NavigationService>().Initialize(ContentFrame);

        foreach (var item in _viewModel.NavigationItems)
        {
            Nav.MenuItems.Add(new NavigationViewItem
            {
                Content = item.Label,
                Icon = new SymbolIcon(ToSymbol(item.Icon)),
                Tag = item.PageType,
            });
        }

        if (Nav.MenuItems.Count > 0 && Nav.MenuItems[0] is NavigationViewItem first)
        {
            Nav.SelectedItem = first;
            ContentFrame.Navigate((Type)first.Tag);
        }
    }

    private void Nav_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer is NavigationViewItem { Tag: Type pageType })
        {
            App.GetService<NavigationService>().Navigate(pageType);
        }
    }

    // Translates the UI-framework-agnostic NavigationIcon (defined in VantageFlow.Core) to a
    // real WinUI Symbol. Only the shell knows about this mapping — modules never reference Symbol.
    private static Symbol ToSymbol(NavigationIcon icon) => icon switch
    {
        NavigationIcon.List => Symbol.List,
        _ => Symbol.Placeholder,
    };
}
