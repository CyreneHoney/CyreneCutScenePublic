using CyreneGUI.Components;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;

namespace CyreneGUI.Views.About;

public sealed partial class AboutPage : Page
{
    private readonly AboutViewModel ViewModel = ModelManager.AboutModel;

    public AboutPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        ViewModel.RefreshVisibility();
        ViewModel.InitGallery(GalleryCanvas);
        SizeChanged += (s, e) => ViewModel.UpdateGalleryPositions(ActualWidth, ActualHeight);
        ViewModel.UpdateGalleryPositions(ActualWidth, ActualHeight);
    }

    private void Grid_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        var props = e.GetCurrentPoint(null).Properties;
        var delta = props.MouseWheelDelta;
        ViewModel.HandleScroll(delta);
        ViewModel.UpdateGalleryPositions(ActualWidth, ActualHeight);
    }
}