using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Frontend.ViewModels;

namespace Frontend.Views;

public partial class AssetsTabView : UserControl
{
    public AssetsTabView()
    {
        InitializeComponent();
    }

    private void OnMainAreaLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is Control mainArea) mainArea.AddHandler(DragDrop.DropEvent, MainArea_Drop);
    }

    private async void MainArea_Drop(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            var firstFile = files?.FirstOrDefault();

            if (firstFile != null && firstFile.Name.EndsWith(".csv", System.StringComparison.OrdinalIgnoreCase))
            {
                var filePath = firstFile.Path.LocalPath;
                if (DataContext is AssetsTabViewModel vm) await vm.ImportAssets(filePath);
            }
        }
    }
}