using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Frontend.Data;
using Frontend.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Frontend.ViewModels;

public class ResultsTabViewModel : INotifyPropertyChanged
{
    private readonly OptimizedResultsClient _client;
    
    public ObservableCollection<DateTimePoint> HeatChartData { get; } = new();
    public ObservableCollection<DateTimePoint> ElectricityChartData { get; } = new();
    public ObservableCollection<DateTimePoint> CO2ChartData { get; } = new();
    public ObservableCollection<DateTimePoint> PrimaryEnergyChartData { get; } = new();
    public ObservableCollection<DateTimePoint> CostChartData { get; } = new();
    public Dictionary<string, ObservableCollection<DateTimePoint>> HeatByGeneratorData { get; } = new();
    public ISeries[] HeatChartSeries =>
    [
        new LineSeries<DateTimePoint>
        {
            Name = "Heat",
            Values = HeatChartData
        }
    ];
    public ISeries[] ElectricityChartSeries =>
    [
        new LineSeries<DateTimePoint>
        {
            Name = "Electricity",
            Values = ElectricityChartData
        }
    ];
    public ISeries[] CO2ChartSeries =>
    [
        new LineSeries<DateTimePoint>
        {
            Name = "CO₂",
            Values = CO2ChartData
        }
    ];
    
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
    HeatChartData.Clear();
    ElectricityChartData.Clear();
    CO2ChartData.Clear();
    PrimaryEnergyChartData.Clear();
    CostChartData.Clear();
    if (SelectedOptimizedResult == null)
        return;

    _allRows = SelectedOptimizedResult.ResultsForHours
        .OrderBy(r => r.TimeFrom)
        .Select(resultList =>
        {
            var heat = resultList.Results.Sum(r => r.HeatProduction);
            var electricity = resultList.Results.Sum(r => r.Electricity);
            var co2 = resultList.Results.Sum(r => r.CO2Produced);
            var primaryEnergy = resultList.Results.Sum(r => r.PrimaryEnergyConsumed);
            var cost = resultList.Results.Sum(r => r.ProductionCost);
            HeatChartData.Add(new DateTimePoint(resultList.TimeFrom, heat));
            ElectricityChartData.Add(new DateTimePoint(resultList.TimeFrom, electricity));
            CO2ChartData.Add(new DateTimePoint(resultList.TimeFrom, co2));
            PrimaryEnergyChartData.Add(new DateTimePoint(resultList.TimeFrom, primaryEnergy));
            CostChartData.Add(new DateTimePoint(resultList.TimeFrom, cost));
            return new ResultTableRow
            {
                Hour = resultList.TimeFrom.ToString("dd.MM.yyyy HH:mm"),
                ActiveAssets = string.Join(
                    ", ",
                    resultList.Results
                        .Select(r => r.Asset?.Name)
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .Distinct()
                ),
                HeatProduced = heat,        
                Electricity = electricity,  
                CO2Produced = co2           
            };
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
    public Axis[] TimeAxis =>
    [
        new Axis
        {
            Labeler = value =>
                {
                    
                    var ticks = (long)value;

                    if (ticks < DateTime.MinValue.Ticks ||
                        ticks > DateTime.MaxValue.Ticks)
                        return string.Empty;

                    return new DateTime(ticks, DateTimeKind.Utc)
                        .ToString("dd.MM.yyyy HH:mm");

                },

            SeparatorsPaint = new SolidColorPaint(SKColors.LightGray),
            ShowSeparatorLines = true,
            MinStep = TimeSpan.FromHours(1).Ticks,
            LabelsRotation = -45
        }
    ];
}