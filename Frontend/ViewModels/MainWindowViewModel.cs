using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Threading;
using Frontend.Data;
using Microsoft.AspNetCore.SignalR.Client;
using Tmds.DBus.Protocol;

namespace Frontend.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private HubConnection? _connection;
    public SourceTabViewModel SourceTab {get; private set;}
    public AssetsTabViewModel AssetsTab  {get; private set;}
    public ResultsTabViewModel ResultTab   {get; private set;}

    public MainWindowViewModel(
        SourceClient sourceClient,
        AssetClient assetClient,
        OptimizerClient optimizerClient,
        OptimizedResultsClient optimizedResultsClient)
    {
        SourceTab = new SourceTabViewModel(sourceClient);
        AssetsTab = new AssetsTabViewModel(assetClient, optimizerClient);
        ResultTab = new ResultsTabViewModel(optimizedResultsClient);
        _ = InitializeSignalRAsync();
    }

    private async Task InitializeSignalRAsync()
    {
        try
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

                    _ = message switch
                    {
                        "Asset" => AssetsTab.LoadFromBackendAsync(),
                        "Source" => SourceTab.LoadAsync(),
                        "Optimized" => ResultTab.LoadAsync(),
                        _ => null
                    };
                });
            });

            await _connection.StartAsync();
            Console.WriteLine("Connected to SignalR Hub!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR connection failed: {ex.Message}");
        }
    }
}