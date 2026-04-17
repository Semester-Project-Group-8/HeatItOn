using Frontend.Data;

namespace Frontend.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public SourceTabViewModel SourceTab { get; }
    public AssetsTabViewModel AssetsTab { get; }
    public ResultsTabViewModel ResultTab { get; }

    public MainWindowViewModel(SourceClient sourceClient, AssetClient assetClient, OptimizerClient optimizerClient,
        OptimizedResultsClient optimizedResultsClient)
    {
        System.Diagnostics.Debug.WriteLine(">>> MainWindowViewModel(SourceClient,AssetClient,OptimizerClient,ResultListClient) ctor");
        SourceTab = new SourceTabViewModel(sourceClient);
        
        AssetsTab = new AssetsTabViewModel(sourceClient, assetClient, optimizerClient);
        ResultTab = new ResultsTabViewModel(optimizedResultsClient);
    }
}