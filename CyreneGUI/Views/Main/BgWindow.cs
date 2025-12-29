using CyreneGUI.Utils;
using CyreneGUI.Views.About;
using CyreneGUI.Views.Home;
using Microsoft.UI.Xaml;
using Windows.Media.Playback;

namespace CyreneGUI.Views.Main;

public sealed partial class MainWindow
{
    private MediaPlayer? BgMusicPlayer;
    private MediaPlayer? BgVideoPlayer;

    private void InitBgMusic()
    {
        if (!File.Exists(AppUtil.GetAsset(AppConst.BgMusic))) return;

        BgMusicPlayer = new MediaPlayer
        {
            Source = AppUtil.GetMedia(AppConst.BgMusic),
            IsLoopingEnabled = true,
            Volume = AppConfigUtil.Config.UI.Volume / 1000.0
        };
        SettingModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(SettingModel.Volume))
                BgMusicPlayer.Volume = SettingModel.Volume / 1000.0;
        };

        BgMusicPlayer.Play();
    }

    private void InitBgVideo()
    {
        if (!File.Exists(AppUtil.GetAsset(AppConst.BgVideo))) return;

        BgVideoPlayer = new MediaPlayer
        {
            Source = AppUtil.GetMedia(AppConst.BgVideo),
            IsLoopingEnabled = true,
            IsMuted = true
        };
        BgVideo.SetMediaPlayer(BgVideoPlayer);
        BgVideoPlayer.Play();
    }

    private void UpdateBgVideoBlur(Type pageType)
    {
        if (BgVideoBorder == null) return;

        BgVideoBorder.Opacity = pageType switch
        {
            Type t when t == typeof(HomePage) => 1,
            Type t when t == typeof(AboutPage) => 0.2,
            _ => 0.4,
        };
        BgVideoBorder.OpacityTransition = new ScalarTransition
        {
            Duration = TimeSpan.FromMilliseconds(300)
        };
    }
}