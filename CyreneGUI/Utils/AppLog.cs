using CyreneCore.Utils;
using CyreneGUI.Components;
using CyreneGUI.Components.Notify;
using CyreneGUI.Views.Console;

namespace CyreneGUI.Utils;

public class AppLog : Logger
{
    public readonly NotifyViewModel NotifyModel = ModelManager.NotifyModel;
    public readonly ConsoleViewModel ConsoleModel = ModelManager.ConsoleModel;

    protected override void Log(string message, LogLevel level)
    {
        base.Log(message, level);

        NotifyModel.ShowNotify(message, level);
        ConsoleModel.AddConsoleLog(message);
    }
}