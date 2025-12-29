using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media.Animation;
using CyreneGUI.Utils;
using CyreneGUI.Components;
using CyreneGUI.Views.Setting;
using CyreneGUI.Views.Home;
using CyreneGUI.Views.Work;
using CyreneGUI.Views.Console;
using CyreneGUI.Views.About;
using CyreneCore.Utils;

namespace CyreneGUI.Views.Main;

public sealed partial class MainWindow : Window
{
    private readonly SettingViewModel SettingModel = ModelManager.SettingModel;

    public MainWindow()
    {
        InitializeComponent();
    }

    public void InitWindow()
    {
        Title = CoreConst.Name;
        AppWindow.SetIcon(AppUtil.GetAsset("Cyrene.ico"));

        SetTitleBar(AppTitleBar);
        ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

        WindowUtil.InitWindowSetting(this);

        NavigateToPage(typeof(HomePage));
        NavView.SelectedItem = NavView.MenuItems[0];

        InitSplashVideo();
        InitBgMusic();
        InitBgVideo();
        RefreshPuzzleState();
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        NavigateToPage(args.SelectedItemContainer.Tag);
    }

    private void NavigateToPage(object rawTag)
    {
        if (rawTag is not string tag) return;

        var pageType = tag switch
        {
            nameof(HomePage) => typeof(HomePage),
            nameof(WorkPage) => typeof(WorkPage),
            nameof(ConsolePage) => typeof(ConsolePage),
            nameof(SettingPage) => typeof(SettingPage),
            nameof(AboutPage) => typeof(AboutPage),
            _ => null
        };
        if (pageType == null || ContentFrame.CurrentSourcePageType == pageType) return;

        ContentFrame.Navigate(pageType, null, new DrillInNavigationTransitionInfo());
        UpdateBgVideoBlur(pageType);
    }
}