using CyreneGUI.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Media.Playback;

namespace CyreneGUI.Views.Main;

public sealed partial class MainWindow
{
    private bool InTrans;
    private bool LoopHidden;
    private MediaPlayer? LoopPlayer;
    private MediaPlayer? TransPlayer;

    private void InitSplashVideo()
    {
        if (AppConfigUtil.Config.UI.SkipSplash ||
            !File.Exists(AppUtil.GetAsset(AppConst.SplashLoop)) ||
            !File.Exists(AppUtil.GetAsset(AppConst.SplashTrans)))
        {
            HideSplashOverlay();
            return;
        }

        LoopPlayer = new MediaPlayer
        {
            Source = new MediaPlaybackList
            {
                AutoRepeatEnabled = true,
                Items = { new MediaPlaybackItem(AppUtil.GetMedia(AppConst.SplashLoop)) }
            }
        };
        LoopPlayer.MediaFailed += (s, e) => AppUtil.TryEnqueue(HideSplashOverlay);
        Loop.SetMediaPlayer(LoopPlayer);
        LoopPlayer.Play();

        TransPlayer = new MediaPlayer
        {
            Source = AppUtil.GetMedia(AppConst.SplashTrans)
        };
        TransPlayer.MediaEnded += (s, e) => AppUtil.TryEnqueue(PlayFadeOut);
        TransPlayer.MediaFailed += (s, e) => AppUtil.TryEnqueue(PlayFadeOut);
        TransPlayer.PlaybackSession.Position = TimeSpan.FromMilliseconds(50); // Skip 50ms
        TransPlayer.PlaybackSession.PlaybackStateChanged += (s, e) =>
        {
            AppUtil.TryEnqueue(() =>
            {
                if (LoopHidden || s.PlaybackState != MediaPlaybackState.Playing) return;

                LoopHidden = true;
                TransPlayer.Pause();
                Loop?.Visibility = Visibility.Collapsed;
                TransPlayer.Play();
            });
        };

        Transition.SetMediaPlayer(TransPlayer);
    }

    private void SplashOverlay_Tapped()
    {
        if (InTrans) return;
        InTrans = true;

        if (TransPlayer != null)
            TransPlayer.Play();
        else
        {
            MediaLayer.Visibility = Visibility.Collapsed;
            PlayFadeOut();
        }
    }

    private void PlayFadeOut()
    {
        AppUtil.AnimateDouble(SplashOverlay, "Opacity", 1.0, 0.0, 800, EasingMode.EaseIn);
        AppUtil.AnimateDouble(SplashScaleTransform, "ScaleX", 1.0, 1.2, 800, EasingMode.EaseIn);
        AppUtil.AnimateDouble(SplashScaleTransform, "ScaleY", 1.0, 1.2, 800, EasingMode.EaseIn, HideSplashOverlay);
    }

    private void HideSplashOverlay()
    {
        SplashOverlay.Visibility = Visibility.Collapsed;

        try
        {
            Loop?.SetMediaPlayer(null);
            Transition?.SetMediaPlayer(null);
        }
        catch
        {
            // Ignore
        }

        if (LoopPlayer != null)
        {
            try
            {
                LoopPlayer.Pause();
                LoopPlayer.Dispose();
            }
            catch
            {
                // Ignore
            }
            LoopPlayer = null;
        }

        if (TransPlayer != null)
        {
            try
            {
                TransPlayer.Pause();
                TransPlayer.Dispose();
            }
            catch
            {
                // Ignore
            }
            TransPlayer = null;
        }
    }

    public bool IsInSplash()
    {
        return SplashOverlay.Visibility == Visibility.Visible;
    }
}