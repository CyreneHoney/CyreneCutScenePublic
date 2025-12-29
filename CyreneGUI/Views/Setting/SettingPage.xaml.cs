using CyreneGUI.Components;
using CyreneGUI.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.Windows.AppLifecycle;

namespace CyreneGUI.Views.Setting;

public sealed partial class SettingPage : Page
{
    public readonly SettingViewModel ViewModel = ModelManager.SettingModel;

    public SettingPage()
    {
        InitializeComponent();
    }

    private async void OnBrowse(string propName, bool isFile = false)
    {
        var result = isFile ? await AppUtil.BrowseSingleFile([".json"]) : await AppUtil.BrowseFloder();
        if (string.IsNullOrEmpty(result)) return;

        ViewModel.ChangeValue(propName, result);
    }

    private void BrowseKey_Click() => OnBrowse(nameof(ViewModel.KeyPath), true);

    private void BrowseFfmpeg_Click() => OnBrowse(nameof(ViewModel.FfmpegPath));

    private void BrowseRes_Click() => OnBrowse(nameof(ViewModel.ResPath));

    private void BrowseLog_Click() => OnBrowse(nameof(ViewModel.LogPath));

    private void BrowseOutput_Click() => OnBrowse(nameof(ViewModel.OutputPath));

    private void Convert_Toggled(object sender, RoutedEventArgs args)
    {
        if (sender is not ToggleSwitch toggle) return;
        ViewModel.ChangeValue(nameof(ViewModel.Convert), toggle.IsOn);
    }

    private void MergeAudio_Toggled(object sender, RoutedEventArgs args)
    {
        if (sender is not ToggleSwitch toggle) return;
        ViewModel.ChangeValue(nameof(ViewModel.MergeAudio), toggle.IsOn);
    }

    private void MergeCaption_Toggled(object sender, RoutedEventArgs args)
    {
        if (sender is not ToggleSwitch toggle) return;
        ViewModel.ChangeValue(nameof(ViewModel.MergeCaption), toggle.IsOn);
    }

    private void SkipSplash_Toggled(object sender, RoutedEventArgs args)
    {
        if (sender is not ToggleSwitch toggle) return;
        ViewModel.ChangeValue(nameof(ViewModel.SkipSplash), toggle.IsOn);
    }

    private void Volume_ValueChanged(object sender, RangeBaseValueChangedEventArgs args)
    {
        ViewModel.ChangeValue(nameof(ViewModel.Volume), args.NewValue);
    }

    private void Restart_Clicked()
    {
        AppInstance.Restart("");
    }

    private void Language_SelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (sender is not ComboBox cb) return;
        ViewModel.ChangeLanguage(cb.SelectedValue);
    }
}
