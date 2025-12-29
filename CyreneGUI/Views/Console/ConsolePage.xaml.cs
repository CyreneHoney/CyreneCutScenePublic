using Microsoft.UI.Xaml.Controls;
using CyreneGUI.Components;

namespace CyreneGUI.Views.Console;

public sealed partial class ConsolePage : Page
{
    public readonly ConsoleViewModel ViewModel = ModelManager.ConsoleModel;

    public ConsolePage()
    {
        InitializeComponent();
    }

    private void LogListView_Loaded()
    {
        if (ViewModel.Logs.Count > 0)
            LogListView.ScrollIntoView(ViewModel.Logs[^1]);
    }

    private void Clear_Click()
    {
        ViewModel.ClearConsoleLog();
    }
}
