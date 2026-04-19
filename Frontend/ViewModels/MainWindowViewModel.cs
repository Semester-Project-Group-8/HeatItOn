using System;
using System.Collections.Generic;
using Frontend.Data;

namespace Frontend.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
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
    }

    public void Refresh()
    {
        _tabs.ForEach(t => t.Refresh());
    }
}