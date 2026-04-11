using Avalonia.Controls;
using Frontend.ViewModels;
namespace Frontend.Views;

public partial class SourceTabView : UserControl
{
    public SourceTabView()
    {
        InitializeComponent();
        DataContext = new SourceTabViewModel();
    }
}