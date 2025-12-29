using Microsoft.UI.Xaml;
using CyreneGUI.Utils;
using CyreneGUI.Views.Main;

namespace CyreneGUI;

public sealed partial class App : Application
{
    public static MainWindow? Window { get; private set; }

    public App()
    {
        InitializeComponent();
        RequestedTheme = ApplicationTheme.Light; // Disable dark mode
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        AppConfigUtil.Init();

        Window = new MainWindow();
        Window.InitWindow();
        Window.Activate();
    }
}
