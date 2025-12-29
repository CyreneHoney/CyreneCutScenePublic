using CyreneGUI.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace CyreneGUI.Components.Notify;

public sealed partial class NotifyPanel : UserControl
{
    public readonly NotifyViewModel ViewModel = ModelManager.NotifyModel;

    public NotifyPanel()
    {
        InitializeComponent();
    }

    private void InfoBar_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not InfoBar infoBar) return;
        if (infoBar.RenderTransform is not TranslateTransform transform) return;

        AppUtil.AnimateDouble(transform, "Y", -100, 0, 400, EasingMode.EaseOut);
        AppUtil.AnimateDouble(infoBar, "Opacity", 0, 1, 400);
    }
}
