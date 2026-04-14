using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Frontend.Data;
using Frontend.Models;

namespace Frontend.ViewModels;

public class ResultsTabViewModel : INotifyPropertyChanged
{
    private readonly OptimizedResultsClient _client;
    public ObservableCollection<OptimizedResults> OptimizedResults { get; } = new();
    private OptimizedResults? _selectedOptimizedResult;
    private List<ResultTableRow> _allRows = new();
    public ObservableCollection<ResultTableRow> Rows { get; } = new();
    public bool HasNoOptimizedResults => OptimizedResults.Count == 0;
    public bool IsResultSelected => SelectedOptimizedResult != null;

    public OptimizedResults? SelectedOptimizedResult
    {
        get => _selectedOptimizedResult;
        set
        {
            if (_selectedOptimizedResult != value)
            {
                _selectedOptimizedResult = value;
                OnPropertyChanged();
                RebuildRows();
                OnPropertyChanged(nameof(IsResultSelected));
            }
        }
    }

    public ResultsTabViewModel(OptimizedResultsClient client)
    {
        _client = client;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var results = await _client.GetAll();
        OptimizedResults.Clear();
        foreach (var r in results) OptimizedResults.Add(r);
        OnPropertyChanged(nameof(HasNoOptimizedResults));
    }

    private void RebuildRows()
    {
        Rows.Clear();
        _allRows.Clear();
        if (SelectedOptimizedResult == null)
            return;
        _allRows = SelectedOptimizedResult.ResultsForHours
            .OrderBy(r => r.TimeFrom)
            .Select(resultList => new ResultTableRow
            {
                Hour = resultList.TimeFrom.ToString("dd.MM.yyyy HH:mm"),
                ActiveAssets = string.Join(
                    ", ",
                    resultList.Results
                        .Select(r => r.Asset?.Name)
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .Distinct()
                ),
                HeatProduced = resultList.Results.Sum(r => r.HeatProduction),
                Electricity = resultList.Results.Sum(r => r.Electricity),
                CO2Produced = resultList.Results.Sum(r => r.CO2Produced)
            })
            .ToList();
        foreach (var row in _allRows)
            Rows.Add(row);
    }

    public void ApplySearch(DateTime from, DateTime to)
    {
        ApplyRows(_allRows.Where(r =>
        {
            var time = DateTime.ParseExact(
                r.Hour, "dd.MM.yyyy HH:mm", null);
            return time >= from && time <= to;
        }));
    }

    public void ClearSearch()
    {
        ApplyRows(_allRows);
    }

    private void ApplyRows(IEnumerable<ResultTableRow> rows)
    {
        Rows.Clear();
        foreach (var row in rows)
            Rows.Add(row);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ResultTableRow
    {
        public string Hour { get; set; } = string.Empty;
        public string ActiveAssets { get; set; } = string.Empty;
        public float HeatProduced { get; set; }
        public float Electricity { get; set; }
        public int CO2Produced { get; set; }
    }
}