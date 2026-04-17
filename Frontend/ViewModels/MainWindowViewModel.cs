using System;
using Frontend.Data;

namespace Frontend.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public AssetsTabViewModel AssetsTab { get; }
    public SourceTabViewModel SourceTab { get; }
    public AssetsTabViewModel AssetsTab { get; }

    public MainWindowViewModel(SourceClient sourceClient, AssetClient assetClient)
    {
        System.Diagnostics.Debug.WriteLine(">>> MainWindowViewModel(SourceClient) ctor");
        AssetsTab = new AssetsTabViewModel(assetClient);
        SourceTab = new SourceTabViewModel(sourceClient);
        AssetsTab = new AssetsTabViewModel(assetClient);
    }
    public MainWindowViewModel()
    {
        throw new Exception("DEFAULT MainWindowViewModel ctor CALLED");
    }

}