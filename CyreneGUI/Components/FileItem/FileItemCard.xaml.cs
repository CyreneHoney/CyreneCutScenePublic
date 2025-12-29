using Microsoft.UI.Xaml.Controls;

namespace CyreneGUI.Components.FileItem;

public sealed partial class FileItemCard : UserControl
{
    private FileItemViewModel ViewModel => (FileItemViewModel)DataContext;

    public FileItemCard()
    {
        InitializeComponent();
        DataContextChanged += (s, e) => Bindings.Update();
    }

    private void Remove_Click()
    {
        ViewModel.ViewModel.RemoveFileItem(ViewModel.FilePath);
    }
}