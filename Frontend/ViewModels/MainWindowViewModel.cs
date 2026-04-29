using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Threading;
using Frontend.Data;
using Microsoft.AspNetCore.SignalR.Client;

namespace Frontend.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private HubConnection? _connection;
    private readonly List<IRefreshable> _tabs = [];
    public SourceTabViewModel SourceTab => _tabs[0] as SourceTabViewModel ?? throw new InvalidOperationException();
    public AssetsTabViewModel AssetsTab => _tabs[1] as AssetsTabViewModel ?? throw new InvalidOperationException();
    public ResultsTabViewModel ResultTab => _tabs[2] as ResultsTabViewModel ?? throw new InvalidOperationException();

    public MainWindowViewModel(
        SourceClient sourceClient,
        AssetClient assetClient,
        OptimizerClient optimizerClient,
        OptimizedResultsClient optimizedResultsClient)
    {
        _tabs.Add(new SourceTabViewModel(sourceClient));
        _tabs.Add(new AssetsTabViewModel(sourceClient, assetClient, optimizerClient));
        _tabs.Add(new ResultsTabViewModel(optimizedResultsClient));

        _ = InitializeSignalRAsync();
    }

    public void Refresh()
    {
        foreach (var tab in _tabs)
            tab.Refresh();
    }

    private async Task InitializeSignalRAsync()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:8080/datahub")
            .WithAutomaticReconnect()
            .Build();

        _connection.On<string>("ReceiveMessage", async message =>
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Debug.WriteLine($"Signal received: {message}. Refreshing UI data...");

                switch (message)
                {
                    case "Asset":
                        AssetsTab.Refresh();
                        break;
                    case "Source":
                        SourceTab.Refresh();
                        break;
                    case "Result":
                    case "ResultList":
                    case "OptimizedResults":
                        ResultTab.Refresh();
                        break;
                    default:
                        Refresh();
                        break;
                }
            });
        });

        try
        {
            await _connection.StartAsync();
            Debug.WriteLine("Connected to SignalR Hub!");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SignalR connection failed: {ex.Message}");
        }
    }
}