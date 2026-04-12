using System;
using Frontend.Data;

namespace Frontend.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public SourceTabViewModel SourceTab { get; }

    public MainWindowViewModel(SourceClient sourceClient)
    {
        System.Diagnostics.Debug.WriteLine(">>> MainWindowViewModel(SourceClient) ctor");
        SourceTab = new SourceTabViewModel(sourceClient);
    }
    public MainWindowViewModel()
    {
        throw new Exception("DEFAULT MainWindowViewModel ctor CALLED");
    }

}