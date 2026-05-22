using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using Frontend.ViewModels;

namespace Frontend.Views;

public partial class ManagerButtonView : UserControl
{
    public ManagerButtonView()
    {
        InitializeComponent();
    }

    private async void ImportAssetsButton_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Assets CSV",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("CSV Files") { Patterns = new[] { "*.csv" } } }
        });

        if (files.Count >= 1)
        {
            string filePath = files[0].Path.LocalPath;

            var parentView = this.FindAncestorOfType<AssetsTabView>();
            if (parentView?.DataContext is AssetsTabViewModel vm)
            {
                await vm.ImportAssets(filePath);

                if (this.DataContext is ManagerButtonViewModel managerVm)
                {
                    managerVm.CancelCommand?.Execute(null);
                }
            }
        }
    }
}