using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Threading;
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

public class ResultsTabViewModel : INotifyPropertyChanged
{
    private readonly OptimizedResultsClient _client;
    private List<ResultTableRow> _allRows = [];
    private OptimizedResults? _selectedOptimizedResult;

    public ResultsTabViewModel(OptimizedResultsClient client)
    {
        _client = client;
        _ = LoadAsync();
    }

    private int _currentPage = 1;
    private const int PageSize = 12;
    private List<ResultTableRow> _filteredRows = [];

    public ObservableCollection<OptimizedResults> OptimizedResults { get; } = [];
    public ObservableCollection<ResultTableRow> Rows { get; } = [];
    public bool HasNoOptimizedResults => OptimizedResults.Count == 0;
    public bool IsResultSelected => SelectedOptimizedResult != null;

    public int CurrentPage => _currentPage;
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(_filteredRows.Count / (double)PageSize));
    public bool CanGoPrev => _currentPage > 1;
    public bool CanGoNext => _currentPage < TotalPages;
    public string PageInfo => _filteredRows.Count == 0
        ? "No rows to display"
        : $"Showing {(_currentPage - 1) * PageSize + 1}-{Math.Min(_currentPage * PageSize, _filteredRows.Count)} of {_filteredRows.Count} rows";
    public List<int> PageNumbers => Enumerable.Range(1, TotalPages).ToList();

    public void NextPage()
    {
        if (!CanGoNext) return;
        _currentPage++;
        NotifyPageChange();
        RefreshPagedRows();
    }

    public void PrevPage()
    {
        if (!CanGoPrev) return;
        _currentPage--;
        NotifyPageChange();
        RefreshPagedRows();
    }

    public void GoToPage(object? page)
    {
        if (page is not int p) return;
        if (p < 1 || p > TotalPages) return;
        _currentPage = p;
        NotifyPageChange();
        RefreshPagedRows();
    }

    private void NotifyPageChange()
    {
        OnPropertyChanged(nameof(CurrentPage));
        OnPropertyChanged(nameof(CanGoPrev));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(PageInfo));
        OnPropertyChanged(nameof(PageNumbers));
    }

    private void RefreshPagedRows()
    {
        Rows.Clear();
        foreach (var row in _filteredRows.Skip((_currentPage - 1) * PageSize).Take(PageSize))
            Rows.Add(row);
    }

    public string TotalHeatDisplay
    {
        get
        {
            var total = _filteredRows.Sum(r => r.HeatProduced);
            return $"{total:F1} MW";
        }
    }

    public string NetElectricityDisplay
    {
        get
        {
            var total = _filteredRows.Sum(r => (double)r.Electricity);
            return total >= 0 ? $"+{total:F1} MW" : $"{total:F1} MW";
        }
    }

    public string TotalCo2Display
    {
        get
        {
            var total = _filteredRows.Sum(r => r.Co2Produced);
            return $"{total:N0} KG";
        }
    }

    public string TotalCostDisplay
    {
        get
        {
            var total = _filteredRows.Sum(r => (double)r.ProductionCost);
            if (Math.Abs(total) >= 1000)
                return $"{total / 1000:F0}K DKK";
            return $"{total:F0} DKK";
        }
    }

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
    private ObservableCollection<ISeries> HeatGeneratorStackedChartData { get; } = [];
    private ObservableCollection<ISeries> Co2GeneratorStackedChartData { get; } = [];
    private ObservableCollection<ISeries> CostGeneratorStackedChartData { get; } = [];
    private ObservableCollection<ISeries> GeneratorUsageChartData { get; } = [];
    public ObservableCollection<GeneratorLegendItem> GeneratorUsageLegend { get; } = new();
    private static readonly string[] GeneratorPalette =
    [
        "#FF6B6B",
        "#4D96FF",
        "#6BCB77",
        "#FFD93D",
        "#845EC2",
        "#00C2A8",
        "#FF8FAB"
    ];

    public ISeries[] HeatChartSeries
    {
        get
        {
            if (HeatGeneratorStackedChartData.Count > 0)
                return HeatGeneratorStackedChartData.ToArray();

            return
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
        }
    }

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

    public ISeries[] Co2ChartSeries
    {
        get
        {
            if (Co2GeneratorStackedChartData.Count > 0)
                return Co2GeneratorStackedChartData.ToArray();

            return
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
        }
    }

    public ISeries[] CostChartSeries
    {
        get
        {
            if (CostGeneratorStackedChartData.Count > 0)
                return CostGeneratorStackedChartData.ToArray();

            return
            [
                new LineSeries<DateTimePoint>
                {
                    Name = "Cost",
                    Values = CostChartData.OrderBy(p => p.DateTime.Ticks).ToArray(),
                    GeometrySize = 0,
                    GeometryStroke = null,
                    GeometryFill = null,
                    Stroke = new SolidColorPaint(SKColor.Parse("#6D28D9"))
                    {
                        StrokeThickness = 2
                    }
                }
            ];
        }
    }

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

    public Axis[] HeatAxis => CreateValueAxis("Heat (MWh)", minLimit: 0);
    public Axis[] ElectricityAxis => CreateValueAxis("Electricity (MWh)", minLimit: null);
    public Axis[] Co2Axis => CreateValueAxis("CO₂ (kg)", minLimit: 0);
    public Axis[] CostAxis => CreateValueAxis("Production cost (DKK)", minLimit: 0);

    public event PropertyChangedEventHandler? PropertyChanged;


    public async Task LoadAsync()
    {
        var results = await _client.GetAll();
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            OptimizedResults.Clear();
            if (results != null)
                foreach (var r in results)
                    OptimizedResults.Add(r);
            OnPropertyChanged(nameof(HasNoOptimizedResults));
        });
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

        var hours = (SelectedOptimizedResult.ResultsForHours ?? [])
            .OrderBy(r => r.TimeFrom)
            .ToList();

        RebuildCharts(hours);
        RebuildGeneratorUsagePie(hours);
        _allRows = (SelectedOptimizedResult.ResultsForHours ?? [])
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
                            .Select(r => r.Asset?.Name)
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

        ApplyRows(_allRows);
    }

    private void RebuildCharts(IEnumerable<ResultList> hours)
    {
        var orderedHours = hours.OrderBy(h => h.TimeFrom).ToList();

        HeatChartData.Clear();
        ElectricityChartData.Clear();
        Co2ChartData.Clear();
        CostChartData.Clear();
        foreach (var hour in orderedHours)
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

        RebuildHeatStackedByGenerator(orderedHours);
        RebuildStackedByGenerator(
            orderedHours,
            Co2GeneratorStackedChartData,
            hour => hour.Results.Sum(r => (double)r.CO2Produced),
            result => result.CO2Produced,
            "kg",
            "CO₂");
        RebuildCostStackedByGenerator(orderedHours);

        OnPropertyChanged(nameof(HeatChartSeries));
        OnPropertyChanged(nameof(ElectricityChartSeries));
        OnPropertyChanged(nameof(Co2ChartSeries));
        OnPropertyChanged(nameof(CostChartSeries));
    }

    private void RebuildHeatStackedByGenerator(IReadOnlyList<ResultList> hours)
    {
        RebuildStackedByHeatDispatch(
            hours,
            HeatGeneratorStackedChartData,
            "Heat",
            (heat, _) => heat);
    }

    private void RebuildCostStackedByGenerator(IReadOnlyList<ResultList> hours)
    {
        RebuildStackedByHeatDispatch(
            hours,
            CostGeneratorStackedChartData,
            "Cost",
            (heat, generator) => heat * generator.CostPerMwh);
    }

    private sealed class DispatchGenerator
    {
        public required int AssetId { get; init; }
        public required string Name { get; init; }
        public required double Capacity { get; init; }
        public required double CostPerMwh { get; init; }
        public required int Order { get; init; }
        public required string HexColor { get; init; }
    }

    private static double PositiveOrDefault(double value, double fallback)
    {
        return value > 0 ? value : fallback;
    }

    private void RebuildStackedByHeatDispatch(
        IReadOnlyList<ResultList> hours,
        ObservableCollection<ISeries> targetSeries,
        string chartLabel,
        Func<double, DispatchGenerator, double> metricFromHeat)
    {
        targetSeries.Clear();

        var grouped = hours
            .SelectMany(hour => hour.Results)
            .GroupBy(result => new
            {
                result.AssetId,
                AssetName = result.Asset?.Name
            })
            .Select(group =>
            {
                var first = group.First();
                var totalHeat = group.Sum(r => (double)r.HeatProduction);
                var observedMaxHeat = group.Max(r => (double)r.HeatProduction);
                var configuredMaxHeat = (double)(first.Asset?.MaxHeat ?? 0f);
                var configuredCost = (double)(first.Asset?.ProductionCost ?? 0);
                var weightedCostNumerator = group.Sum(r => (double)r.ProductionCost);
                var weightedCostDenominator = group.Sum(r => (double)r.HeatProduction);
                var weightedCostPerMwh = weightedCostDenominator > 0
                    ? weightedCostNumerator / weightedCostDenominator
                    : 0d;

                return new
                {
                    first.AssetId,
                    Name = string.IsNullOrWhiteSpace(group.Key.AssetName)
                        ? $"Asset {group.Key.AssetId}"
                        : group.Key.AssetName!,
                    TotalHeat = totalHeat,
                    Capacity = PositiveOrDefault(configuredMaxHeat, observedMaxHeat),
                    Merit = PositiveOrDefault(configuredCost, weightedCostPerMwh),
                    CostPerMwh = PositiveOrDefault(weightedCostPerMwh, configuredCost)
                };
            })
            .Where(item => item.TotalHeat > 0 && item.Capacity > 0)
            .OrderBy(item => item.Merit)
            .ThenByDescending(item => item.Capacity)
            .ToList();

        if (grouped.Count == 0)
            return;

        var generators = grouped
            .Select((item, index) => new DispatchGenerator
            {
                AssetId = item.AssetId,
                Name = item.Name,
                Capacity = item.Capacity,
                CostPerMwh = item.CostPerMwh,
                Order = index,
                HexColor = GeneratorPalette[index % GeneratorPalette.Length]
            })
            .ToList();

        var valuesByGenerator = generators.ToDictionary(
            generator => generator.AssetId,
            _ => new List<double>(hours.Count));

        var totalValues = new List<double>(hours.Count);

        foreach (var hour in hours)
        {
            var demand = (double)hour.Results.Sum(result => result.HeatProduction);
            var remaining = Math.Max(0d, demand);
            var hourTotalMetric = 0d;

            foreach (var generator in generators)
            {
                var dispatchedHeat = Math.Min(generator.Capacity, remaining);
                remaining -= dispatchedHeat;

                var metricValue = Math.Max(0d, metricFromHeat(dispatchedHeat, generator));
                valuesByGenerator[generator.AssetId].Add(metricValue);
                hourTotalMetric += metricValue;
            }

            totalValues.Add(hourTotalMetric);
        }

        foreach (var generator in generators)
        {
            var points = hours
                .Select((hour, index) => new DateTimePoint(
                    hour.TimeFrom,
                    valuesByGenerator[generator.AssetId][index]))
                .ToArray();

            targetSeries.Add(new StackedAreaSeries<DateTimePoint>
            {
                Name = generator.Name,
                Values = points,
                ScalesYAt = 0,
                GeometrySize = 0,
                GeometryStroke = null,
                GeometryFill = null,
                Stroke = new SolidColorPaint(SKColor.Parse(generator.HexColor))
                {
                    StrokeThickness = 0.9f
                },
                Fill = new SolidColorPaint(SKColor.Parse($"44{generator.HexColor.Substring(1)}"))
            });
        }

        targetSeries.Add(new LineSeries<DateTimePoint>
        {
            Name = $"{chartLabel} total",
            Values = hours
                .Select((hour, index) => new DateTimePoint(hour.TimeFrom, totalValues[index]))
                .ToArray(),
            GeometrySize = 0,
            GeometryStroke = null,
            GeometryFill = null,
            Fill = null,
            Stroke = new SolidColorPaint(SKColor.Parse("#1F2937"))
            {
                StrokeThickness = 1f
            }
        });
    }

    private void RebuildStackedByGenerator(
        IReadOnlyList<ResultList> hours,
        ObservableCollection<ISeries> targetSeries,
        Func<ResultList, double> totalSelector,
        Func<Result, float> valueSelector,
        string unitLabel,
        string chartLabel)
    {
        targetSeries.Clear();

        var totalPoints = hours
            .Select(hour => new DateTimePoint(hour.TimeFrom, totalSelector(hour)))
            .ToArray();

        var generatorOrder = hours
            .SelectMany(hour => hour.Results)
            .GroupBy(result => new
            {
                result.AssetId,
                AssetName = result.Asset?.Name
            })
            .Select(group => new
            {
                group.Key.AssetId,
                Name = string.IsNullOrWhiteSpace(group.Key.AssetName)
                    ? $"Asset {group.Key.AssetId}"
                    : group.Key.AssetName!,
                TotalValue = group.Sum(result => (double)valueSelector(result))
            })
            .Where(item => item.TotalValue > 0)
            .OrderByDescending(item => item.TotalValue)
            .ToList();

        for (var index = 0; index < generatorOrder.Count; index++)
        {
            var generator = generatorOrder[index];
            var hex = GeneratorPalette[index % GeneratorPalette.Length];

            var points = hours
                .Select(hour => new DateTimePoint(
                    hour.TimeFrom,
                    hour.Results
                        .Where(result => result.AssetId == generator.AssetId)
                        .Sum(result => (double)valueSelector(result))))
                .ToArray();

            targetSeries.Add(new StackedAreaSeries<DateTimePoint>
            {
                Name = generator.Name,
                Values = points,
                ScalesYAt = 0,
                GeometrySize = 0,
                GeometryStroke = null,
                GeometryFill = null,
                Stroke = new SolidColorPaint(SKColor.Parse(hex))
                {
                    StrokeThickness = 0.9f
                },
                Fill = new SolidColorPaint(SKColor.Parse($"44{hex.Substring(1)}"))
            });
        }

        if (generatorOrder.Count > 0)
        {
            targetSeries.Add(new LineSeries<DateTimePoint>
            {
                Name = $"{chartLabel} total",
                Values = totalPoints,
                GeometrySize = 0,
                GeometryStroke = null,
                GeometryFill = null,
                Fill = null,
                Stroke = new SolidColorPaint(SKColor.Parse("#1F2937"))
                {
                    StrokeThickness = 1f
                }
            });
        }
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

        GeneratorUsageLegend.Clear();

        for (var index = 0; index < assetTotals.Count; index++)
        {
            var item = assetTotals[index];
            var percentage = item.Value / total * 100d;
            var hideLabel = percentage < 2d;

            // add legend item (color swatch + name)
            var hex = GetPieColor(item.Name, index);
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

    private static string GetPieColor(string generatorName, int index)
    {
        var normalized = new string(generatorName.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

        return normalized switch
        {
            "GASBOILER1" or "GB1" => "#FF6B6B",
            "GASBOILER2" or "GB2" => "#4D96FF",
            "GASBOILER3" or "GB3" => "#6BCB77",
            _ => GeneratorPalette[index % GeneratorPalette.Length]
        };
    }

    public void ApplySearch(DateTime from, DateTime to)
    {
        if (SelectedOptimizedResult == null)
            return;
        var filteredHours = (SelectedOptimizedResult.ResultsForHours ?? [])
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
        _filteredRows = rows.ToList();
        _currentPage = 1;
        OnPropertyChanged(nameof(TotalPages));
        NotifyPageChange();
        RefreshPagedRows();
        OnPropertyChanged(nameof(TotalHeatDisplay));
        OnPropertyChanged(nameof(NetElectricityDisplay));
        OnPropertyChanged(nameof(TotalCo2Display));
        OnPropertyChanged(nameof(TotalCostDisplay));
    }

    private static Axis[] CreateValueAxis(string name, double? minLimit = 0)
    {
        return
        [
            new()
            {
                Name = name,
                NameTextSize = 12,
                TextSize = 12,
                MinLimit = minLimit,
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
