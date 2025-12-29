using CommunityToolkit.Mvvm.ComponentModel;
using CyreneCore.Utils;
using CyreneGUI.Views.Work;

namespace CyreneGUI.Components.FileItem;

public sealed partial class FileItemViewModel(string filePath) : ObservableObject
{
    [ObservableProperty] public partial string FilePath { get; set; } = filePath;
    [ObservableProperty] public partial string FileName { get; set; } = Path.GetFileNameWithoutExtension(filePath);
    [ObservableProperty] public partial ProcessState State { get; set; }
    [ObservableProperty] public partial string? ErrorTip { get; set; }
    public readonly WorkViewModel ViewModel = ModelManager.WorkModel;
}
