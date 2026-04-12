using Frontend.Data;
using Frontend.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Frontend.ViewModels;

public class ResultsTabViewModel : ViewModelBase
{
    private readonly ResultListClient _resultListClient;
    private static readonly string[] SupportedDateFormats =
    {
        "yyyy-MM-dd HH:mm",
        "yyyy-MM-dd"
    };

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
            if (string.IsNullOrWhiteSpace(SearchFrom) && string.IsNullOrWhiteSpace(SearchTo))
            {
                RebuildTableForSelection();
            }
        }
    }

    private string _searchFrom = string.Empty;
    public string SearchFrom
    {
        get => _searchFrom;
        set
        {
            _searchFrom = value;
            OnPropertyChanged();
        }
    }

    private string _searchTo = string.Empty;
    public string SearchTo
    {
        get => _searchTo;
        set
        {
            _searchTo = value;
            OnPropertyChanged();
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

    public void ApplySearch()
    {
        if (!TryParseDateTime(SearchFrom, out var from) || !TryParseDateTime(SearchTo, out var to))
        {
            StatusMessage = "Use format: yyyy-MM-dd HH:mm";
            return;
        }

        if (from.HasValue && to.HasValue && from.Value > to.Value)
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
        SearchFrom = string.Empty;
        SearchTo = string.Empty;
        StatusMessage = string.Empty;
        RebuildTableForSelection();
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

    private static IEnumerable<ResultList> FilterByPeriod(IEnumerable<ResultList> source, DateTime? from, DateTime? to)
    {
        return source.Where(resultList =>
        {
            var rowFrom = resultList.TimeFrom;
            var rowTo = resultList.TimeTo == default ? resultList.TimeFrom.AddHours(1) : resultList.TimeTo;

            var fromCheck = !to.HasValue || rowFrom <= to.Value;
            var toCheck = !from.HasValue || rowTo >= from.Value;

            return fromCheck && toCheck;
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

    private static bool TryParseDateTime(string value, out DateTime? parsed)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            parsed = null;
            return true;
        }

        var trimmed = value.Trim();

        if (DateTime.TryParseExact(trimmed, SupportedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
        {
            parsed = exact;
            return true;
        }

        if (DateTime.TryParse(trimmed, out var fallback))
        {
            parsed = fallback;
            return true;
        }

        parsed = null;
        return false;
    }

}

public class ResultTableRow
{
    public string Hour { get; set; } = string.Empty;
    public string ActiveGenerators { get; set; } = string.Empty;
    public float ElectricityConsumed { get; set; }
    public float TotalNetPrice { get; set; }
}