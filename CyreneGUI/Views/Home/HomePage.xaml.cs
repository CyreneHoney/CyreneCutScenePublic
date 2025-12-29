using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using CyreneGUI.Components;

namespace CyreneGUI.Views.Home;

public sealed partial class HomePage : Page
{
    public readonly HomeViewModel ViewModel = ModelManager.HomeModel;

    public HomePage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.StartTypeWelcome();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        ViewModel.StopTypeWelcome();
    }

    private void Morse_PointerPressed()
    {
        ViewModel.PressMorse();
    }

    private void Morse_PointerReleased()
    {
        ViewModel.ReleaseMorse();
    }

    private void Morse_PointerExited()
    {
        ViewModel.ExitMorse();
    }
}