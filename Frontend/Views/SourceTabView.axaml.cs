using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Frontend.Models;
using Frontend.ViewModels;
using System.Linq;

namespace Frontend.Views;

public partial class SourceTabView : UserControl
{
    public SourceTabView()
    {
        InitializeComponent();
    }

    private void SourceDataGrid_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Commit)
            if (e.Row.DataContext is Source source && DataContext is SourceTabViewModel vm)
                vm.UpdateSource(source);
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
                if (DataContext is SourceTabViewModel vm) await vm.Import(filePath);
            }
        }
    }

    private async void ImportSourceButton_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Source CSV",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("CSV Files") { Patterns = new[] { "*.csv" } } }
        });

        if (files.Count >= 1)
        {
            var filePath = files[0].Path.LocalPath;
            if (DataContext is SourceTabViewModel vm) await vm.Import(filePath);
        }
    }
}