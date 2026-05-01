using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using Frontend.Data;
using Frontend.Data.CSV;
using Frontend.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.Measure;

namespace Frontend.ViewModels;

public class SourceTabViewModel : ViewModelBase
public partial class SourceTabViewModel :
    ViewModelBase,
    IRefreshable
{
    // Api connection
    private readonly SourceClient _client;

    // Sources
    private readonly ObservableCollection<Source> _allSources = [];
    public ObservableCollection<Source> Sources { get; } = [];
    public ObservableCollection<Source> PagedSources { get; } = [];
    private Source? _selectedSource;

    // Pagination
    private int _currentPage = 1;
    private const int PageSize = 15;

    public int TotalPages => Math.Max(1, (int)Math.Ceiling(Sources.Count / (double)PageSize));
    public bool CanGoPrev => _currentPage > 1;
    public bool CanGoNext => _currentPage < TotalPages;
    public string PageInfo => Sources.Count == 0
        ? "No rows to display"
        : $"Showing {(_currentPage - 1) * PageSize + 1}-{Math.Min(_currentPage * PageSize, Sources.Count)} of {Sources.Count} rows";
    public List<int> PageNumbers => Enumerable.Range(1, TotalPages).ToList();

    public void NextPage()
    {
        if (!CanGoNext) return;
        _currentPage++;
        NotifyPageChange();
        RefreshPagedSources();
    }

    public void PrevPage()
    {
        if (!CanGoPrev) return;
        _currentPage--;
        NotifyPageChange();
        RefreshPagedSources();
    }

    public void GoToPage(object? page)
    {
        if (page is not int p) return;
        if (p < 1 || p > TotalPages) return;
        _currentPage = p;
        NotifyPageChange();
        RefreshPagedSources();
    }

    private void NotifyPageChange()
    {
        OnPropertyChanged(nameof(CurrentPage));
        OnPropertyChanged(nameof(CanGoPrev));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(PageInfo));
        OnPropertyChanged(nameof(PageNumbers));
    }

    public int CurrentPage => _currentPage;

    private void RefreshPagedSources()
    {
        PagedSources.Clear();
        foreach (var s in Sources.Skip((_currentPage - 1) * PageSize).Take(PageSize))
            PagedSources.Add(s);
    }

    // Uploaded files
    public ObservableCollection<string> Files { get; } = [];
    private string? _selectedFile;

    // Charts
    public ObservableCollection<ISeries> WinterSeries { get; } = [];
    public ObservableCollection<ISeries> SummerSeries { get; } = [];
    public Axis[] TimeAxis { get; }
    public Axis[] DualAxes { get; }
    public bool HasSources => Sources.Count > 0;

    public string? SelectedFile
    {
        get => _selectedFile;
        set
        {
            if (SetProperty(ref _selectedFile, value))
                SelectFile();
        }
    }

    public Source? SelectedSource
    {
        get => _selectedSource;
        set => SetProperty(ref _selectedSource, value);
    }

    public SourceTabViewModel(SourceClient client)
    {
        _client = client;
        TimeAxis =
        [
            new Axis
            {
                Name = "Time",

                Labeler = value =>
                {
                    var ticks = (long)value;

                    if (ticks < DateTime.MinValue.Ticks)
                        ticks = DateTime.MinValue.Ticks;
                    else if (ticks > DateTime.MaxValue.Ticks)
                        ticks = DateTime.MaxValue.Ticks;

                    return new DateTime(ticks, DateTimeKind.Utc)
                        .ToString("dd.MM HH:mm");
                },
                UnitWidth = TimeSpan.FromHours(1).Ticks,
                MinStep = TimeSpan.FromHours(20).Ticks,
                ForceStepToMin = true,
                MinZoomDelta = TimeSpan.FromHours(3).Ticks,
                LabelsRotation = -90,
                TextSize = 11,
                NameTextSize = 10,
                SeparatorsPaint = new SolidColorPaint(SKColors.LightGray),
                ShowSeparatorLines = true,
            }
        ];
        DualAxes =
        [
            new Axis
            {
                Name = "Heat Demand (MWh)",
                Position = AxisPosition.Start,
                TextSize = 14,
                NameTextSize = 10,
                MinLimit = 0,
                MaxLimit = null
            },
            new Axis
            {
                Name = "Electricity Price (DKK/MWh)",
                Position = AxisPosition.End,
                TextSize = 14,
                NameTextSize = 10,
                MinLimit = 0,
                MaxLimit = null
            }
        ];
        _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        try
        {
            var sources = await _client.GetAll() ?? [];

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _allSources.Clear();
                Sources.Clear();
                Files.Clear();

                foreach (var source in sources)
                {
                    source.FileName ??= "source.csv";
                    _allSources.Add(source);

                    if (!Files.Contains(source.FileName))
                        Files.Add(source.FileName);
                }

                SelectedFile = Files.FirstOrDefault();
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void SelectFile()
    {
        Sources.Clear();

        if (SelectedFile is null)
            return;

        foreach (var source in _allSources.Where(s => s.FileName == SelectedFile))
            Sources.Add(source);

        _currentPage = 1;
        NotifyPageChange();
        RefreshPagedSources();

        Dispatcher.UIThread.Post(() =>
        {
            BuildWinterSeries();
            BuildSummerSeries();
        });
    }

    public void Export()
    {
        SourceCsvHandler.ExportCsv(Path.Combine(AppContext.BaseDirectory, "exported_source.csv"), Sources.ToList());
    }

    public void Import()
    {
            _ = SourceCsvHandler.ImportCsv(Path.Combine(AppContext.BaseDirectory, "source.csv"), _client);
    }

    private static bool IsWinter(Source s)
    {
        return s.TimeFrom.Month is <= 3 or >= 10;
    }

    private static bool IsSummer(Source s)
    {
        return s.TimeFrom.Month is >= 4 and <= 9;
    }

    private void BuildWinterSeries()
    {
        WinterSeries.Clear();

        var winter = Sources
            .Where(IsWinter)
            .OrderBy(x => x.TimeFrom)
            .ToList();

        if (winter.Count == 0)
            return;

        WinterSeries.Add(new LineSeries<DateTimePoint>
        {
            Values = winter.Select(x =>
                new DateTimePoint(x.TimeFrom, x.HeatDemand)).ToList(),
            GeometrySize = 0,
            GeometryStroke = null,
            GeometryFill = null,
            ScalesYAt = 0,
            // heat uses red
            Stroke = new SolidColorPaint(SKColor.Parse("#E4572E")) { StrokeThickness = 1 },
            Fill = new SolidColorPaint(SKColor.Parse("#30E4572E"))
        });

        WinterSeries.Add(new LineSeries<DateTimePoint>
        {
            Values = winter.Select(x =>
                new DateTimePoint(x.TimeFrom, x.ElectricityPrice)).ToList(),
            GeometrySize = 0,
            GeometryStroke = null,
            GeometryFill = null,
            ScalesYAt = 1,
            // electricity uses blue
            Stroke = new SolidColorPaint(SKColor.Parse("#0084FF")) { StrokeThickness = 1 },
            Fill = new SolidColorPaint(SKColor.Parse("#300084FF"))
        });

        
    }

    private void BuildSummerSeries()
    {
        SummerSeries.Clear();

        var summer = Sources
            .Where(IsSummer)
            .OrderBy(x => x.TimeFrom)
            .ToList();

        if (summer.Count == 0)
            return;

        SummerSeries.Add(new LineSeries<DateTimePoint>
        {
            Values = summer.Select(x =>
                new DateTimePoint(x.TimeFrom, x.HeatDemand)).ToList(),
            GeometrySize = 0,
            GeometryStroke = null,
            GeometryFill = null,
            ScalesYAt = 0,
            Stroke = new SolidColorPaint(SKColor.Parse("#E4572E")) { StrokeThickness = 1 },
            Fill = new SolidColorPaint(SKColor.Parse("#30E4572E"))
        });

        SummerSeries.Add(new LineSeries<DateTimePoint>
        {
            Values = summer.Select(x =>
                new DateTimePoint(x.TimeFrom, x.ElectricityPrice)).ToList(),
            GeometrySize = 0,
            GeometryStroke = null,
            GeometryFill = null,
            ScalesYAt = 1,
            Stroke = new SolidColorPaint(SKColor.Parse("#0084FF")) { StrokeThickness = 1 },
            Fill = new SolidColorPaint(SKColor.Parse("#300084FF"))
        });

        
    }
}