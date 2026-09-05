using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VantageFlow;

/// <summary>
/// The OS window: title bar and the root Frame, nothing else. It navigates straight to
/// ShellPage, which owns navigation; feature pages never navigate through here directly.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        AppWindow.SetIcon("Assets/AppIcon.ico");

        // Navigate the root frame to the shell on startup.
        RootFrame.Navigate(typeof(ShellPage));
    }
}
