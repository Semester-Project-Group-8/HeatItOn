using Frontend.Data;
using Frontend.Data.CSV;
using System;

namespace Frontend.ViewModels;

public class AssetsTabViewModel : ViewModelBase
{
    private readonly AssetClient _assetClient;

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public AssetsTabViewModel(AssetClient assetClient)
    {
        _assetClient = assetClient;
    }

    public void ExportCsv()
    {
        try
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = System.IO.Path.Combine(desktopPath, "Assets_Export.csv");

            AssetCsvHandler.ExportCsv(filePath, _assetClient);

            StatusMessage = "Exported successfully to Desktop!";
        }
        catch (Exception ex)
        {
            StatusMessage = "Export failed: " + ex.Message;
        }
    }

    public void ImportCsv()
    {
        try
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = System.IO.Path.Combine(desktopPath, "Assets_Import.csv");

            AssetCsvHandler.ImportCsv(filePath, _assetClient);

            StatusMessage = "Imported successfully from Desktop!";
        }
        catch (Exception ex)
        {
            StatusMessage = "Import failed: " + ex.Message;
        }
    }
}