using Frontend.Data;
using Frontend.Models;
using System.Collections.Generic;
namespace Frontend.ViewModels;

public class AssetsTabViewModel : ViewModelBase
{
    static SourceClient _sourceClient;
    static AssetClient _assetClient;
    static OptimizerClient _optimizerClient;
    List<Asset> assets;
    public AssetsTabViewModel(SourceClient sourceClient,AssetClient assetClient,OptimizerClient optimizerClient)
    {
        _sourceClient = sourceClient;
        _assetClient = assetClient;
        _optimizerClient = optimizerClient;
    }

    public async void Optimize()
    {
        await _optimizerClient.Optimize();
    }
}