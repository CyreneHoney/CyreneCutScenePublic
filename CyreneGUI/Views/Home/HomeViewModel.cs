using CommunityToolkit.Mvvm.ComponentModel;
using CyreneGUI.Utils;
using Microsoft.UI.Dispatching;

namespace CyreneGUI.Views.Home;

public sealed partial class HomeViewModel : ObservableObject
{
    private DispatcherQueueTimer? TypeTimer;
    private DispatcherQueueTimer? CursorTimer;
    private DateTime PressStartTime;
    private DispatcherQueueTimer? MorseTimer;
    [ObservableProperty] public partial string WelcomeText { get; set; }= "";
    [ObservableProperty] public partial bool IsMorseTipOpen { get; set; }
    [ObservableProperty] public partial string MorseInput { get; set; } = "";

    public async void StartTypeWelcome()
    {
        while (App.Window!.IsInSplash())
            await Task.Delay(2000);
        StopTypeWelcome();

        WelcomeText = "";
        var typingText = "";
        var fullText = AppConst.Welcome.RandomElement()!;

        TypeTimer = AppUtil.CreateTimer();
        TypeTimer.Tick += (s, e) =>
        {
            var curIndex = typingText.Length;
            if (curIndex >= fullText.Length)
            {
                WelcomeText = typingText;
                StopTypeWelcome();
                return;
            }

            typingText += fullText[curIndex];
            TypeTimer.Interval = TimeSpan.FromMilliseconds(Random.Shared.Next(200, 500));
        };
        TypeTimer.Start();

        CursorTimer = AppUtil.CreateTimer(500);
        CursorTimer.Tick += (s, e) =>
        {
            if (typingText == fullText)
            {
                WelcomeText = typingText;
                StopTypeWelcome();
                return;
            }

            var curVisible = WelcomeText.EndsWith('┃');
            WelcomeText = curVisible ? typingText : typingText + "┃";
        };
        CursorTimer.Start();
    }

    public async void StopTypeWelcome()
    {
        TypeTimer?.Stop();
        CursorTimer?.Stop();
        TypeTimer = null;
        CursorTimer = null;
        await Task.Delay(500); // Wait tick finish
    }

    private void VerifyMorse()
    {
        if (MorseInput.Length > AppConst.SYTMorse.Length)
        {
            ExitMorse();
            return;
        }

        if (MorseInput == AppConst.SYAMorse)
        {
            App.Window!.RefreshPuzzleState(PuzzleState.SYALocked);
            ExitMorse();
        }
        else if (MorseInput == AppConst.SYTMorse)
        {
            App.Window!.RefreshPuzzleState(PuzzleState.SYTLocked);
            ExitMorse();
        }
    }

    public void PressMorse()
    {
        if (AppConfigUtil.Config.UI.Puzzle != PuzzleState.None) return;

        StopMorseTimer();
        IsMorseTipOpen = true;

        PressStartTime = DateTime.Now;
        MorseTimer = AppUtil.CreateTimer(500);
        MorseTimer.IsRepeating = false;
        MorseTimer.Tick += (s, e) =>
        {
            if (IsMorseTipOpen)
            {
                MorseInput += "-";
                VerifyMorse();
            }

            StopMorseTimer();
        };
        MorseTimer.Start();
    }

    public void ReleaseMorse()
    {
        if (MorseTimer == null) return;
        StopMorseTimer();

        if (IsMorseTipOpen)
        {
            MorseInput += (DateTime.Now - PressStartTime).TotalSeconds > 0.5 ? "-" : ".";
            VerifyMorse();
        }
    }

    public async void ExitMorse()
    {
        StopMorseTimer();

        await Task.Delay(300);
        IsMorseTipOpen = false;
        MorseInput = "";
    }

    private void StopMorseTimer()
    {
        try
        {
            MorseTimer?.Stop();
            MorseTimer = null;
        }
        catch
        {
            // Ignore
        }
    }
}
