using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using Avalonia.Media;

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
    private ObservableCollection<ISeries> GeneratorUsageChartData { get; } = [];
    public ObservableCollection<GeneratorLegendItem> GeneratorUsageLegend { get; } = new();

    public ISeries[] HeatChartSeries =>
    [
        new LineSeries<DateTimePoint>
        {
            Name = "Heat",
            Values = HeatChartData.OrderBy(p => p.DateTime.Ticks).ToArray(),
            GeometrySize = 0,
            GeometryStroke = null,
            GeometryFill = null,
            Stroke = new SolidColorPaint(SKColor.Parse("#E4572E"))
            {
                StrokeThickness = 2
            }
        }
    ];

    public ISeries[] ElectricityChartSeries =>
    [
        new LineSeries<DateTimePoint>
        {
            Name = "Electricity",
            Values = ElectricityChartData.OrderBy(p => p.DateTime.Ticks).ToArray(),
            GeometrySize = 0,
            GeometryStroke = null,
            GeometryFill = null,
            Stroke = new SolidColorPaint(SKColor.Parse("#0084FF"))
            {
                StrokeThickness = 2
            }
        }
    ];

    public ISeries[] Co2ChartSeries =>
    [
        new LineSeries<DateTimePoint>
        {
            Name = "CO₂",
            Values = Co2ChartData.OrderBy(p => p.DateTime.Ticks).ToArray(),
            GeometrySize = 0,
            GeometryStroke = null,
            GeometryFill = null,
            Stroke = new SolidColorPaint(SKColor.Parse("#4B5563"))
            {
                StrokeThickness = 2
            }
        }
    ];

    public ISeries[] GeneratorUsagePieSeries => GeneratorUsageChartData.ToArray();

    public Axis[] TimeAxis =>
    [
        new()
        {
            Name = "Time",
            NameTextSize = 12,
            CrosshairPaint = new SolidColorPaint(SKColor.Parse("#94A3B8"), 1),
            CrosshairSnapEnabled = true,
            Labeler = value =>
            {
                var ticks = (long)value;

                if (ticks < DateTime.MinValue.Ticks ||
                    ticks > DateTime.MaxValue.Ticks)
                    return string.Empty;

                return new DateTime(ticks, DateTimeKind.Utc)
                    .ToString("dd.MM HH:mm");
            },

            SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#D9DDE3")),
            ShowSeparatorLines = true,
            MinStep = TimeSpan.FromHours(2).Ticks,
            TextSize = 12,
            LabelsRotation = -30
        }
    ];

    public Axis[] HeatAxis => CreateValueAxis("Heat (MWh)");
    public Axis[] ElectricityAxis => CreateValueAxis("Electricity (MWh)");
    public Axis[] Co2Axis => CreateValueAxis("CO₂ (kg)");

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
        ResultCsvHandler.ExportCsv(Path.Combine(AppContext.BaseDirectory, "result.csv"), Rows.ToList());
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
        RebuildGeneratorUsagePie(hours);
        _allRows = SelectedOptimizedResult.ResultsForHours
            .OrderBy(r => r.TimeFrom)
            .Select(resultList =>
            {
                var heat = resultList.Results.Sum(r => r.HeatProduction);
                var electricity = resultList.Results.Sum(r => r.Electricity);
                var co2 = resultList.Results.Sum(r => r.CO2Produced);
                var cost = resultList.Results.Sum(r => r.ProductionCost);
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
                    HeatProduced = MathF.Round(heat,2),
                    Electricity = MathF.Round(electricity,2),
                    Co2Produced = co2,
                    ProductionCost = MathF.Round(cost,2)
                    
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

    private void RebuildGeneratorUsagePie(IEnumerable<ResultList> hours)
    {
        GeneratorUsageChartData.Clear();

        var assetTotals = hours
            .SelectMany(hour => hour.Results)
            .GroupBy(result => new
            {
                result.AssetId,
                AssetName = result.Asset?.Name
            })
            .Select(group => new
            {
                Name = string.IsNullOrWhiteSpace(group.Key.AssetName)
                    ? $"Asset {group.Key.AssetId}"
                    : group.Key.AssetName!,
                Value = group.Sum(result => (double)result.HeatProduction)
            })
            .Where(item => item.Value > 0)
            .OrderByDescending(item => item.Value)
            .ToList();

        var total = assetTotals.Sum(item => item.Value);
        if (total <= 0)
        {
            OnPropertyChanged(nameof(GeneratorUsagePieSeries));
            return;
        }

        var colors = new[]
        {
            "#FF8C42",
            "#0084FF",
            "#E4572E",
            "#4CAF50",
            "#9C27B0",
            "#FFC107",
            "#607D8B"
        };

        GeneratorUsageLegend.Clear();

        for (var index = 0; index < assetTotals.Count; index++)
        {
            var item = assetTotals[index];
            var percentage = item.Value / total * 100d;
            var hideLabel = percentage < 2d;

            // add legend item (color swatch + name)
            var hex = colors[index % colors.Length];
            IBrush brush;
            try
            {
                brush = new SolidColorBrush(Color.Parse(hex));
            }
            catch
            {
                brush = Brushes.Gray;
            }

            GeneratorUsageLegend.Add(new GeneratorLegendItem
            {
                Name = item.Name,
                Percentage = Math.Round(percentage, 1),
                Fill = brush
            });

            // show only percentage on the pie; tiny slices hide label
            GeneratorUsageChartData.Add(new PieSeries<double>
            {
                Name = item.Name,
                Values = [percentage],
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsSize = 12,
                DataLabelsFormatter = _ => hideLabel ? string.Empty : $"{percentage:0.#}%",
                DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#FFFFFF")),
                Fill = new SolidColorPaint(SKColor.Parse(hex))
            });
        }

        OnPropertyChanged(nameof(GeneratorUsagePieSeries));
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
        RebuildGeneratorUsagePie(filteredHours);
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

    private static Axis[] CreateValueAxis(string name)
    {
        return
        [
            new()
            {
                Name = name,
                NameTextSize = 12,
                TextSize = 12,
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#D9DDE3")),
                ShowSeparatorLines = true
            }
        ];
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
