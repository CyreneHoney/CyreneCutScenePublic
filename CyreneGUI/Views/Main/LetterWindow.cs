using CyreneGUI.Utils;
using CyreneCore.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Media.Playback;
using Windows.UI;
using static I18N.DotNet.GlobalLocalizer;

namespace CyreneGUI.Views.Main;

public sealed partial class MainWindow : Window
{
    private MediaPlayer? LetterTransPlayer;
    private static PuzzleState State => AppConfigUtil.Config.UI.Puzzle;

    public void RefreshPuzzleState(PuzzleState? state = null)
    {
        if (state != null)
        {
            AppConfigUtil.Config.UI.Puzzle = state.Value;
            AppConfigUtil.Save();
        }

        if (State == PuzzleState.None)
            PuzzleNavItem.Visibility = Visibility.Collapsed;
        else
            PuzzleNavItem.Visibility = Visibility.Visible;

        PuzzleNavItem.Icon = new ImageIcon
        {
            Source = AppUtil.GetImage(State.IsLocked() ? "Lock.svg" : "Puzzle.svg")
        };
    }

    private void Puzzle_Tapped()
    {
        if (State.IsLocked()) VerifyTip.IsOpen = true;
        else if (State.IsUnlocked()) InitLetterOverlay();
    }

    private void Confirm_Click()
    {
        if (!State.IsLocked() || PwdBox.Password != CoreConst.Author)
            Logger.Error(Localize("VerifyFailed"));
        else
        {
            VerifyTip.IsOpen = false;
            RefreshPuzzleState(State.GetUnlockState());
        }
    }

    private void Cancel_Click()
    {
        VerifyTip.IsOpen = false;
    }

    private void InitLetterOverlay()
    {
        if (!File.Exists(AppUtil.GetAsset(AppConst.LetterTrans))) return;

        ResetLetterState();

        LetterTransPlayer = new MediaPlayer
        {
            Source = AppUtil.GetMedia(AppConst.LetterTrans),
            Volume = 1.0,
            IsLoopingEnabled = false,
            AudioCategory = MediaPlayerAudioCategory.Media
        };
        LetterTransPlayer.MediaEnded += (s, args) => AppUtil.TryEnqueue(StartLetterTyping);
        LetterTransPlayer.MediaFailed += (s, args) => AppUtil.TryEnqueue(HideLetterPlayer);

        LetterTrans.SetMediaPlayer(LetterTransPlayer);

        AppUtil.AnimateDouble(LetterOverlay, "Opacity", 0, 1, 1000, EasingMode.EaseOut, () =>
        {
            LetterTransPlayer.Play();
            AppUtil.AnimateDouble(BlackCurtain, "Opacity", 1, 0, 600, EasingMode.EaseIn);
        });
    }

    private static Color CalcLineColor(int curLine, int totalLines)
    {
        var startColor = Color.FromArgb(255, 66, 165, 245); // #42A5F5
        var endColor = Color.FromArgb(255, 240, 98, 146);   // #F06292
        var p = totalLines <= 1 ? 0 : (double)curLine / (totalLines - 1);
        var r = (byte)(startColor.R + (endColor.R - startColor.R) * p);
        var g = (byte)(startColor.G + (endColor.G - startColor.G) * p);
        var b = (byte)(startColor.B + (endColor.B - startColor.B) * p);
        return Color.FromArgb(255, r, g, b);
    }

    private async void StartLetterTyping()
    {
        LetterPanel.Children.Clear();
        LetterScroller.IsHitTestVisible = false;
        LetterContainer.Visibility = Visibility.Visible;

        var lines = State.GetLetter();
        for (int i = 0; i < lines.Count; i++)
        {
            var lineContent = lines[i];
            var textBlock = new TextBlock
            {
                Text = lineContent,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0,
                Visibility = Visibility.Visible,
                FontSize = 16,
                Foreground = new SolidColorBrush(CalcLineColor(i, lines.Count)),
                Margin = new Thickness(0, 0, 0, 6),
                HorizontalAlignment = lineContent.Contains('\t') ? HorizontalAlignment.Right : HorizontalAlignment.Left
            };
            LetterPanel.Children.Add(textBlock);
            LetterScroller.ChangeView(null, LetterScroller.ScrollableHeight, null);

            AppUtil.AnimateDouble(textBlock, "Opacity", 0, 1, 1500, EasingMode.EaseOut);
            await Task.Delay(800);
        }

        LetterScroller.IsHitTestVisible = true;
        LetterCloseButton.Visibility = Visibility.Visible;
    }

    private void LetterClose_Click()
    {
        LetterCloseButton.Visibility = Visibility.Collapsed;
        if (State == PuzzleState.SYAUnlocked) RefreshPuzzleState(PuzzleState.SYAFinished);
        PlayLetterFadeOut();
    }

    private void PlayLetterFadeOut()
    {
        AppUtil.AnimateDouble(BlackCurtain, "Opacity", 0, 1, 600, EasingMode.EaseOut, () =>
        {
            LetterPanel.Children.Clear();
            LetterTrans.SetMediaPlayer(null);
            LetterTrans.Visibility = Visibility.Collapsed;

            AppUtil.AnimateDouble(LetterOverlay, "Opacity", 1, 0, 1000, EasingMode.EaseIn, HideLetterPlayer);
        });
    }

    private void ResetLetterState()
    {
        LetterOverlay.Visibility = Visibility.Visible;
        LetterOverlay.Opacity = 0;
        LetterOverlay.IsHitTestVisible = true;

        BlackCurtain.Opacity = 1;
        LetterTrans.Opacity = 1;
        LetterTrans.Visibility = Visibility.Visible;

        LetterContainer.Opacity = 1;
        LetterCloseButton.Visibility = Visibility.Collapsed;
        LetterCloseButton.IsHitTestVisible = true;
    }

    private void HideLetterPlayer()
    {
        LetterOverlay.Visibility = Visibility.Collapsed;

        if (LetterTransPlayer != null)
        {
            try
            {
                LetterTransPlayer.Pause();
                LetterTransPlayer.Dispose();
            }
            catch
            {
                // Ignore
            }
            LetterTransPlayer = null;
        }
    }
}