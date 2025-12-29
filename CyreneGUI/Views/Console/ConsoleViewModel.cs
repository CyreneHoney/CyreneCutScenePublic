using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using CyreneCore.Utils;
using CyreneGUI.Utils;
using static I18N.DotNet.GlobalLocalizer;

namespace CyreneGUI.Views.Console;

public sealed partial class ConsoleViewModel : ObservableObject
{
    public ObservableCollection<string> Logs { get; } = [];

    public void AddConsoleLog(string message)
    {
        AppUtil.TryEnqueue(() =>
        {
            Logs.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            if (Logs.Count > 1000) Logs.RemoveAt(0);
            Logger.WriteLog(message, LogLevel.Info);
        });
    }

    public void ClearConsoleLog()
    {
        AppUtil.TryEnqueue(() =>
        {
            Logs.Clear();
            Logger.Info(Localize("SuccClearLog"));
        });
    }
}