using Avalonia.Controls;
using Avalonia.Interactivity;
using Frontend.Data;
using Frontend.ViewModels;
using System;
using System.Net.Http;

namespace Frontend.Views;

public partial class ResultsTabView : UserControl
{
    public ResultsTabView()
    {
        InitializeComponent();

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        DataContext = new ResultsTabViewModel(
            new ResultListClient(httpClient));
    }

    private void OnSearchClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ResultsTabViewModel viewModel)
        {
            viewModel.ApplySearch();
        }
    }

    private void OnClearClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ResultsTabViewModel viewModel)
        {
            viewModel.ClearSearch();
        }
    }
}