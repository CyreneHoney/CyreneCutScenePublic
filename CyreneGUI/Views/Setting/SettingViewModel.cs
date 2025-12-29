using CommunityToolkit.Mvvm.ComponentModel;
using CyreneCore.Utils;
using CyreneGUI.Utils;
using Microsoft.UI.Xaml;
using static I18N.DotNet.GlobalLocalizer;

namespace CyreneGUI.Views.Setting;

public sealed partial class SettingViewModel : ObservableObject
{
    [ObservableProperty] public partial string KeyPath { get; set; } = AppConfigUtil.Config.Path.KeyPath;
    [ObservableProperty] public partial string FfmpegPath { get; set; } = AppConfigUtil.Config.Path.FFmpegPath;
    [ObservableProperty] public partial string ResPath { get; set; } = AppConfigUtil.Config.Path.ResPath;
    [ObservableProperty] public partial string LogPath { get; set; } = AppConfigUtil.Config.Path.LogPath;
    [ObservableProperty] public partial string OutputPath { get; set; } = AppConfigUtil.Config.Extract.Output;
    [ObservableProperty] public partial bool Convert { get; set; } = AppConfigUtil.Config.Extract.Convert;
    [ObservableProperty] public partial bool MergeAudio { get; set; } = AppConfigUtil.Config.Extract.MergeAudio;
    [ObservableProperty] public partial bool MergeCaption { get; set; } = AppConfigUtil.Config.Extract.MergeCaption;
    [ObservableProperty] public partial bool SkipSplash { get; set; } = AppConfigUtil.Config.UI.SkipSplash;
    [ObservableProperty] public partial int Volume { get; set; } = AppConfigUtil.Config.UI.Volume;
    [ObservableProperty] public partial Visibility RestartVisibility { get; set; } = Visibility.Collapsed;
    [ObservableProperty] public partial string Language { get; set; } = AppConfigUtil.Config.Language.ToString();

    public SettingViewModel()
    {
        LoadConfig();
    }

    public void ChangeValue(string propName, object value)
    {
        AppUtil.TryEnqueue(() =>
        {
            if (value is bool boolValue)
            {
                _ = propName switch
                {
                    nameof(Convert) => Convert = boolValue,
                    nameof(MergeAudio) => MergeAudio = boolValue,
                    nameof(MergeCaption) => MergeCaption = boolValue,
                    nameof(SkipSplash) => SkipSplash = boolValue,
                    _ => false
                };
            }
            else if (value is int intValue)
            {
                _ = propName switch
                {
                    nameof(Volume) => Volume = intValue,
                    _ => 0
                };
            }
            else if (value is string strValue)
            {
                _ = propName switch
                {
                    nameof(KeyPath) => KeyPath = strValue,
                    nameof(FfmpegPath) => FfmpegPath = strValue,
                    nameof(ResPath) => ResPath = strValue,
                    nameof(LogPath) => LogPath = strValue,
                    nameof(OutputPath) => OutputPath = strValue,
                    nameof(Language) => Language = strValue,
                    _ => ""
                };
            }

            SaveConfig();
        });
    }

    public void ChangeLanguage(object value)
    {
        if (value is not string strValue || strValue == Language) return;

        AppUtil.TryEnqueue(() =>
        {
            ChangeValue(nameof(Language), strValue);
            RestartVisibility = Visibility.Visible;
            Logger.Warn(Localize("ToLoadLang"));
        });
    }

    private void LoadConfig()
    {
        AppUtil.TryEnqueue(() =>
        {
            KeyPath = AppConfigUtil.Config.Path.KeyPath;
            FfmpegPath = AppConfigUtil.Config.Path.FFmpegPath;
            ResPath = AppConfigUtil.Config.Path.ResPath;
            LogPath = AppConfigUtil.Config.Path.LogPath;
            OutputPath = AppConfigUtil.Config.Extract.Output;
            Convert = AppConfigUtil.Config.Extract.Convert;
            MergeAudio = AppConfigUtil.Config.Extract.MergeAudio;
            MergeCaption = AppConfigUtil.Config.Extract.MergeCaption;
            SkipSplash = AppConfigUtil.Config.UI.SkipSplash;
            Volume = AppConfigUtil.Config.UI.Volume;
            Language = AppConfigUtil.Config.Language.ToString();
        });
    }

    public void SaveConfig()
    {
        AppConfigUtil.Config.Path.KeyPath = KeyPath;
        AppConfigUtil.Config.Path.FFmpegPath = FfmpegPath;
        AppConfigUtil.Config.Path.ResPath = ResPath;
        AppConfigUtil.Config.Path.LogPath = LogPath;
        AppConfigUtil.Config.Extract.Output = OutputPath;
        AppConfigUtil.Config.Extract.Convert = Convert;
        AppConfigUtil.Config.Extract.MergeAudio = MergeAudio;
        AppConfigUtil.Config.Extract.MergeCaption = MergeCaption;
        AppConfigUtil.Config.UI.SkipSplash = SkipSplash;
        AppConfigUtil.Config.UI.Volume = Volume;
        AppConfigUtil.Config.Language = Enum.TryParse<LanguageTypeEnum>(Language, out var l) ? l : LanguageTypeEnum.CHS;

        AppConfigUtil.Save();
    }
}