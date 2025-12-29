using CyreneGUI.Components.Notify;
using CyreneGUI.Views.Home;
using CyreneGUI.Views.Console;
using CyreneGUI.Views.Setting;
using CyreneGUI.Views.Work;
using CyreneGUI.Views.About;

namespace CyreneGUI.Components;

public class ModelManager
{
    public static AboutViewModel AboutModel { get; } = new();
    public static ConsoleViewModel ConsoleModel { get; } = new();
    public static HomeViewModel HomeModel { get; } = new();
    public static NotifyViewModel NotifyModel { get; } = new();
    public static SettingViewModel SettingModel { get; } = new();
    public static WorkViewModel WorkModel { get; } = new();
}