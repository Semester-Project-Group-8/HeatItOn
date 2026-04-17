using Frontend.Data;
using Frontend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Frontend.ViewModels;

public class ResultsTabViewModel : ViewModelBase
{
    private readonly ResultListClient _resultListClient;

    private List<ResultList> _resultLists = new();
    public List<ResultList> ResultLists
    {
        get => _resultLists;
        set
        {
            _resultLists = value;
            OnPropertyChanged();
        }
    }

    private ResultList? _selectedResultList;
    public ResultList? SelectedResultList
    {
        get => _selectedResultList;
        set
        {
            _selectedResultList = value;
            OnPropertyChanged();
            RebuildTableForSelection();
        }
    }

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

    private List<ResultTableRow> _tableRows = new();
    public List<ResultTableRow> TableRows
    {
        get => _tableRows;
        set
        {
            _tableRows = value;
            OnPropertyChanged();
        }
    }

    public ResultsTabViewModel(ResultListClient resultListClient)
    {
        _resultListClient = resultListClient;

        _ = LoadResultLists();
    }

    public async Task LoadResultLists()
    {
        try
        {
            ResultLists = (await _resultListClient.ListResultLists() ?? new List<ResultList>())
                .OrderBy(resultList => resultList.TimeFrom)
                .ToList();

            SelectedResultList = ResultLists.FirstOrDefault();
            RebuildTableForSelection();
            StatusMessage = string.Empty;
        }
        catch
        {
            ResultLists = new List<ResultList>();
            TableRows = new List<ResultTableRow>();
            StatusMessage = "Unable to load result lists.";
        }
    }

    public void ApplySearch(DateTime from, DateTime to)
    {
        if (from > to)
        {
            StatusMessage = "From must be earlier than To.";
            return;
        }

        var filtered = FilterByPeriod(ResultLists, from, to).ToList();
        TableRows = BuildRowsForPeriod(filtered);

        StatusMessage = filtered.Count == 0
            ? "No data in selected period."
            : $"Showing {TableRows.Count} hour(s).";
    }

    public void ClearSearch()
    {
        StatusMessage = string.Empty;
        RebuildTableForSelection();
    }

    //test result export
    public void ExportCsv()
    {
        try
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = System.IO.Path.Combine(desktopPath, "Optimized_Results_Export.csv");

            Frontend.Data.CSV.ResultCsvHandler.ExportCsv(filePath, _resultListClient);

            StatusMessage = "Exported successfully to Desktop!";
        }
        catch (Exception ex)
        {
            StatusMessage = "Export failed: " + ex.Message;
        }
    }

    private void RebuildTableForSelection()
    {
        if (SelectedResultList is null)
        {
            TableRows = new List<ResultTableRow>();
            StatusMessage = "No result list selected.";
            return;
        }

        TableRows = new List<ResultTableRow> { BuildRowFromResultList(SelectedResultList) };
        StatusMessage = string.Empty;
    }

    private static IEnumerable<ResultList> FilterByPeriod(IEnumerable<ResultList> source, DateTime from, DateTime to)
    {
        return source.Where(resultList =>
        {
            var rowFrom = resultList.TimeFrom;
            var rowTo = resultList.TimeTo == default ? resultList.TimeFrom.AddHours(1) : resultList.TimeTo;

            return rowFrom <= to && rowTo >= from;
        });
    }

    private static List<ResultTableRow> BuildRowsForPeriod(IEnumerable<ResultList> resultLists)
    {
        return resultLists
            .GroupBy(resultList => new DateTime(resultList.TimeFrom.Year, resultList.TimeFrom.Month, resultList.TimeFrom.Day, resultList.TimeFrom.Hour, 0, 0))
            .OrderBy(group => group.Key)
            .Select(group => BuildRowFromResults(group.Key.ToString("yyyy-MM-dd HH:mm"), group.SelectMany(list => list.Results)))
            .ToList();
    }

    private static ResultTableRow BuildRowFromResultList(ResultList resultList)
    {
        var hour = resultList.TimeFrom == default
            ? $"ResultList #{resultList.Id}"
            : resultList.TimeFrom.ToString("yyyy-MM-dd HH:mm");

        return BuildRowFromResults(hour, resultList.Results);
    }

    private static ResultTableRow BuildRowFromResults(string hour, IEnumerable<Result> results)
    {
        var resultItems = results.ToList();

        var electricityConsumed = resultItems
            .Where(result => result.Electricity < 0)
            .Sum(result => -result.Electricity);

        var totalNetPrice = resultItems.Sum(result => result.ProductionCost);

        return new ResultTableRow
        {
            Hour = hour,
            ActiveGenerators = string.Join(", ", GetActiveGeneratorNames(resultItems)),
            ElectricityConsumed = electricityConsumed,
            TotalNetPrice = totalNetPrice
        };
    }

    private static List<string> GetActiveGeneratorNames(IEnumerable<Result> results)
    {
        return results
            .Where(result => result.HeatProduction > 0 || result.Electricity != 0)
            .Select(result => string.IsNullOrWhiteSpace(result.Asset?.Name)
                ? $"Asset {result.AssetId}"
                : result.Asset.Name)
            .Distinct()
            .ToList();
    }


}

public class ResultTableRow
{
    public string Hour { get; set; } = string.Empty;
    public string ActiveGenerators { get; set; } = string.Empty;
    public float ElectricityConsumed { get; set; }
    public float TotalNetPrice { get; set; }
}