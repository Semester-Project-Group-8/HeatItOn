using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using Frontend.Data;
using Frontend.Data.CSV;
using Frontend.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Measure;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Frontend.ViewModels;

public partial class SourceTabViewModel : 
    ViewModelBase,
    IRefreshable
{
    // Api connection
    private readonly SourceClient _client;

    // Sources
    private readonly ObservableCollection<Source> _allSources = [];
    public ObservableCollection<Source> Sources { get; } = [];
    private Source? _selectedSource;

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
                MinLimit = null,
                MaxLimit = null
            },
            new Axis
            {
                Name = "Electricity Price (DKK/MWh)",
                Position = AxisPosition.End,
                TextSize = 14,
                NameTextSize = 10,
                MinLimit = null,
                MaxLimit = null
            }
        ];
        _ = LoadFromBackend();
    }

    private async Task LoadFromBackend()
    {
        try
        {
            var sources = await _client.GetAll();

            foreach (var source in sources)
            {
                source.FileName ??= "source.csv";
                _allSources.Add(source);

                if (!Files.Contains(source.FileName))
                    Files.Add(source.FileName);
            }

            SelectedFile = Files.FirstOrDefault();
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

        Dispatcher.UIThread.Post(() =>
        {
            BuildWinterSeries();
            BuildSummerSeries();
        });
    }

    public void Export()
    {
        SourceCsvHandler.ExportCsv("source.csv", Sources.ToList());
    }

    public async Task Import()
    {
        await SourceCsvHandler.ImportCsv("source.csv", _client);
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
            Name = "Heat Demand",
            Values = winter.Select(x =>
                new DateTimePoint(x.TimeFrom, x.HeatDemand)).ToList(),
            GeometrySize = 0,
            ScalesYAt = 0
        });

        WinterSeries.Add(new LineSeries<DateTimePoint>
        {
            Name = "Electricity Price",
            Values = winter.Select(x =>
                new DateTimePoint(x.TimeFrom, x.ElectricityPrice)).ToList(),
            GeometrySize = 0,
            ScalesYAt = 1
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
            Name = "Heat Demand",
            Values = summer.Select(x =>
                new DateTimePoint(x.TimeFrom, x.HeatDemand)).ToList(),
            GeometrySize = 0,
            ScalesYAt = 0
        });

        SummerSeries.Add(new LineSeries<DateTimePoint>
        {
            Name = "Electricity Price",
            Values = summer.Select(x =>
                new DateTimePoint(x.TimeFrom, x.ElectricityPrice)).ToList(),
            GeometrySize = 0,
            ScalesYAt = 1
        });
    }

    public void Refresh()
    {
        _ = LoadFromBackend();
    }
}