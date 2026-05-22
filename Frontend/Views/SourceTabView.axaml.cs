using Avalonia.Controls;
using Frontend.ViewModels;
using Frontend.Models;

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
        {
            if (e.Row.DataContext is Source source && DataContext is SourceTabViewModel vm)
            {
                vm.UpdateSource(source);
            }
        }
    }
}