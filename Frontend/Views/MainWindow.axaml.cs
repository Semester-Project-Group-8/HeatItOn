using Avalonia.Controls;
using Frontend.Data;

namespace Frontend.Views;

public partial class MainWindow : Window
{
    public MainWindow(ResultListClient resultListClient)
    {
        InitializeComponent();

        ResultsTabItem.Content = new ResultsTabView(resultListClient);
    }
}