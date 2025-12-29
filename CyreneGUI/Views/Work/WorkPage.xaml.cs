using Microsoft.UI.Xaml.Controls;
using CyreneGUI.Utils;
using CyreneGUI.Components;

namespace CyreneGUI.Views.Work;

public sealed partial class WorkPage : Page
{
    public readonly WorkViewModel ViewModel = ModelManager.WorkModel;

    public WorkPage()
    {
        InitializeComponent();
    }

    private async void BrowseInputFiles_Click()
    {
        ViewModel.AddFileItems(await AppUtil.BrowseMultiFile([".usm"]));
    }

    private async void BrowseInputFolder_Click()
    {
        ViewModel.AddFileItems([await AppUtil.BrowseFloder()]);
    }

    private void ClearInputFiles_Click()
    {
        ViewModel.ClearFileItems();
    }

    private void OpenOutputFolder_Click()
    {
        ViewModel.OpenOutputFloder();
    }

    private async void Process_Click()
    {
        await ViewModel.StartProcessAsync();
    }

    private void Cancel_Click()
    {
        ViewModel.CancelProcess();
    }
}