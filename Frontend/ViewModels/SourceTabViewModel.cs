using System.Threading.Tasks;
using System;
using CommunityToolkit.Mvvm.Input;
using Frontend.Data;
using System.IO;
using System.Net.Http;
namespace Frontend.ViewModels;

public partial class SourceTabViewModel : ViewModelBase  
{
    
    private readonly SourceClient _sourceClient;
    public SourceTabViewModel()
    {
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:8080/");
        _sourceClient = new SourceClient(httpClient);
    }

    [RelayCommand]   
    async Task ImportCsv()
    {
        await _sourceClient.ImportCsv();
        Console.WriteLine("CSV import initiated.");
    }

    [RelayCommand]
    async Task ExportCsv()
    {
        var csv = await _sourceClient.ExportCsv();
        await File.WriteAllTextAsync("exported_heating.csv", csv);
        Console.WriteLine("CSV export completed.");

    }
}

