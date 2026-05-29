using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Threading;
using Frontend.Data;
using Frontend.Interfaces;
using Frontend.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Frontend.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private static readonly TimeSpan SignalRRetryDelay = TimeSpan.FromSeconds(5);
    private HubConnection? _connection;
    public SourceTabViewModel SourceTab { get; private set; }
    public AssetsTabViewModel AssetsTab { get; private set; }
    public ResultsTabViewModel ResultTab { get; private set; }
    public RelayCommand Refresh { get; }

    public MainWindowViewModel(
        IClient<Source> sourceClient,
        IClient<Asset> assetClient,
        OptimizerClient optimizerClient,
        IClient<OptimizedResults> optimizedResultsClient)
    {
        SourceTab = new SourceTabViewModel(sourceClient);
        AssetsTab = new AssetsTabViewModel(assetClient, optimizerClient);
        ResultTab = new ResultsTabViewModel(optimizedResultsClient);
        Refresh = new RelayCommand(() =>
        {
            _ = AssetsTab.LoadFromBackendAsync();
            _ = SourceTab.LoadAsync();
            _ = ResultTab.LoadAsync();
        });
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

            _connection.Reconnected += async _ =>
            {
                Debug.WriteLine("SignalR reconnected. Refreshing UI data...");
                await RefreshAllTabsAsync();
            };

            while (true)
            {
                if (_connection.State is HubConnectionState.Connected or HubConnectionState.Connecting
                    or HubConnectionState.Reconnecting)
                {
                    await Task.Delay(SignalRRetryDelay);
                    continue;
                }

                try
                {
                    await _connection.StartAsync();
                    Console.WriteLine("Connected to SignalR Hub!");
                    await RefreshAllTabsAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"SignalR connection attempt failed: {ex.Message}");
                    await Task.Delay(SignalRRetryDelay);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("SignalR initialization failed", ex);
        }
    }

    private async Task RefreshAllTabsAsync()
    {
        try
        {
            await Task.WhenAll(
                AssetsTab.LoadFromBackendAsync(),
                SourceTab.LoadAsync(),
                ResultTab.LoadAsync());
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Refreshing UI data after SignalR connection failed: {ex.Message}");
        }
    }
}