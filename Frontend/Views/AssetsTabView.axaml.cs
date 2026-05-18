using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Frontend.ViewModels;
using System.Linq;

namespace Frontend.Views;

public partial class AssetsTabView : UserControl
{
    public AssetsTabView()
    {
        InitializeComponent();
    }

    private void startOptimize(object? sender, RoutedEventArgs e)
    {

    }

    private void OnMainAreaLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Control mainArea)
        {
            mainArea.AddHandler(DragDrop.DropEvent, MainArea_Drop);
        }
    }

    private async void MainArea_Drop(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            var firstFile = files?.FirstOrDefault();

            if (firstFile != null && firstFile.Name.EndsWith(".csv", System.StringComparison.OrdinalIgnoreCase))
            {
                string filePath = firstFile.Path.LocalPath;

                if (this.DataContext is AssetsTabViewModel vm)
                {
                    await vm.ImportAssets(filePath);
                }
            }
        }
    }
}