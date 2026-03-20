using Frontend.Data;

namespace Frontend.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public readonly AssetClient AssetClient;
    public readonly SourceClient SourceClient;
    public MainWindowViewModel(AssetClient assetClient, SourceClient sourceClient)
    {
        AssetClient = assetClient;
        SourceClient = sourceClient;
    }
}