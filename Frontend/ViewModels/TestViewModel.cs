using Frontend.Data;

namespace Frontend.ViewModels;

public partial class TestViewModel : ViewModelBase
{
    public readonly AssetClient AssetClient;
    public readonly SourceClient SourceClient;
    public TestViewModel(AssetClient assetClient, SourceClient sourceClient)
    {
        AssetClient = assetClient;
        SourceClient = sourceClient;
    }
}