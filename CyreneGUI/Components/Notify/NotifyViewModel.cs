using CommunityToolkit.Mvvm.ComponentModel;
using CyreneCore.Utils;
using CyreneGUI.Utils;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

namespace CyreneGUI.Components.Notify;

public record NotifyItem(string Message, InfoBarSeverity Severity);

public sealed partial class NotifyViewModel : ObservableObject
{
    public ObservableCollection<NotifyItem> InfoBars { get; } = [];

    private static InfoBarSeverity GetSeverity(LogLevel level)
    {
        return level switch
        {
            LogLevel.Error => InfoBarSeverity.Error,
            LogLevel.Warn => InfoBarSeverity.Warning,
            LogLevel.Info => InfoBarSeverity.Success,
            LogLevel.Debug => InfoBarSeverity.Informational,
            _ => InfoBarSeverity.Informational
        };
    }

    public void ShowNotify(string message, LogLevel level)
    {
        AppUtil.TryEnqueue(() =>
        {
            var item = new NotifyItem(message, GetSeverity(level));

            InfoBars.Insert(0, item);

            var timer = AppUtil.CreateTimer(3000);
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                RemoveNotify(item);
            };
            timer.Start();

            while (InfoBars.Count > 3)
                InfoBars.RemoveAt(InfoBars.Count - 1);
        });
    }

    public void RemoveNotify(NotifyItem item)
    {
        AppUtil.TryEnqueue(() => InfoBars.Remove(item));
    }
}