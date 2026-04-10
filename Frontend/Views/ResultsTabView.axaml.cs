using Avalonia.Controls;
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
            new OptimizerClient(httpClient),
            new ResultClient(httpClient),
            new ResultListClient(httpClient));
    }
}