using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Frontend.Data;
using Frontend.Data.CSV;
using Frontend.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Frontend.ViewModels;

public class ResultsTabViewModel : 
    INotifyPropertyChanged,
    IRefreshable
{
    private readonly OptimizedResultsClient _client;
    private List<ResultTableRow> _allRows = [];
    private OptimizedResults? _selectedOptimizedResult;

    public ResultsTabViewModel(OptimizedResultsClient client)
    {
        _client = client;
        _ = LoadAsync();
    }

    public ObservableCollection<OptimizedResults> OptimizedResults { get; } = [];
    public ObservableCollection<ResultTableRow> Rows { get; } = [];
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

    // Charts 
    private ObservableCollection<DateTimePoint> HeatChartData { get; } = [];
    private ObservableCollection<DateTimePoint> ElectricityChartData { get; } = [];
    private ObservableCollection<DateTimePoint> Co2ChartData { get; } = [];
    private ObservableCollection<DateTimePoint> CostChartData { get; } = [];

    public ISeries[] HeatChartSeries =>
    [
        new LineSeries<DateTimePoint>
        {
            Name = "Heat",
            Values = HeatChartData,
            GeometrySize = 0
        }
    ];

    public ISeries[] ElectricityChartSeries =>
    [
        new LineSeries<DateTimePoint>
        {
            Name = "Electricity",
            Values = ElectricityChartData,
            GeometrySize = 0
        }
    ];

    public ISeries[] Co2ChartSeries =>
    [
        new LineSeries<DateTimePoint>
        {
            Name = "CO₂",
            Values = Co2ChartData,
            GeometrySize = 0
        }
    ];

    public Axis[] TimeAxis =>
    [
        new()
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

    public event PropertyChangedEventHandler? PropertyChanged;


    private async Task LoadAsync()
    {
        var results = await _client.GetAll();
        OptimizedResults.Clear();
        if (results != null)
            foreach (var r in results)
                OptimizedResults.Add(r);
        OnPropertyChanged(nameof(HasNoOptimizedResults));
    }

    public void Export()
    {
        ResultCsvHandler.ExportCsv("result.csv", Rows.ToList());
    }

    private void RebuildRows()
    {
        Rows.Clear();
        _allRows.Clear();

        if (SelectedOptimizedResult == null)
            return;

        var hours = SelectedOptimizedResult.ResultsForHours
            .OrderBy(r => r.TimeFrom)
            .ToList();

        RebuildCharts(hours);
        _allRows = SelectedOptimizedResult.ResultsForHours
            .OrderBy(r => r.TimeFrom)
            .Select(resultList =>
            {
                var heat = resultList.Results.Sum(r => r.HeatProduction);
                var electricity = resultList.Results.Sum(r => r.Electricity);
                var co2 = resultList.Results.Sum(r => r.CO2Produced);
                var cost = resultList.Results.Sum(r => r.ProductionCost);
                HeatChartData.Add(new DateTimePoint(resultList.TimeFrom, heat));
                ElectricityChartData.Add(new DateTimePoint(resultList.TimeFrom, electricity));
                Co2ChartData.Add(new DateTimePoint(resultList.TimeFrom, co2));
                CostChartData.Add(new DateTimePoint(resultList.TimeFrom, cost));
                return new ResultTableRow
                {
                    Hour = resultList.TimeFrom.ToString("dd.MM.yyyy HH:mm"),
                    ActiveAssets = string.Join(
                        "; ",
                        resultList.Results
                            .Select(r => r.Asset.Name)
                            .Where(n => !string.IsNullOrWhiteSpace(n))
                            .Distinct()
                    ),
                    HeatProduced = heat,
                    Electricity = electricity,
                    Co2Produced = co2,
                    ProductionCost = cost
                    
                };
            })
            .ToList();

        foreach (var row in _allRows)
            Rows.Add(row);
    }

    private void RebuildCharts(IEnumerable<ResultList> hours)
    {
        HeatChartData.Clear();
        ElectricityChartData.Clear();
        Co2ChartData.Clear();
        CostChartData.Clear();
        foreach (var hour in hours.OrderBy(h => h.TimeFrom))
        {
            var heat = hour.Results.Sum(r => r.HeatProduction);
            var electricity = hour.Results.Sum(r => r.Electricity);
            var co2 = hour.Results.Sum(r => r.CO2Produced);
            var cost = hour.Results.Sum(r => r.ProductionCost);

            HeatChartData.Add(new DateTimePoint(hour.TimeFrom, heat));
            ElectricityChartData.Add(new DateTimePoint(hour.TimeFrom, electricity));
            Co2ChartData.Add(new DateTimePoint(hour.TimeFrom, co2));
            CostChartData.Add(new DateTimePoint(hour.TimeFrom, cost));
        }

        OnPropertyChanged(nameof(HeatChartSeries));
        OnPropertyChanged(nameof(ElectricityChartSeries));
        OnPropertyChanged(nameof(Co2ChartSeries));
    }

    public void ApplySearch(DateTime from, DateTime to)
    {
        if (SelectedOptimizedResult == null)
            return;
        var filteredHours = SelectedOptimizedResult.ResultsForHours
            .Where(h => h.TimeFrom >= from && h.TimeFrom <= to)
            .OrderBy(h => h.TimeFrom)
            .ToList();
        RebuildCharts(filteredHours);
        ApplyRows(_allRows.Where(r =>
        {
            var time = DateTime.ParseExact(
                r.Hour, "dd.MM.yyyy HH:mm", null);
            return time >= from && time <= to;
        }));
    }

    public void ClearSearch()
    {
        RebuildRows();
    }

    private void ApplyRows(IEnumerable<ResultTableRow> rows)
    {
        Rows.Clear();
        foreach (var row in rows)
            Rows.Add(row);
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public void Refresh()
    {
        _ = LoadAsync();
    }
}
