using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Reflection;
using Frontend.Data;
using Frontend.Models;
using Frontend.ViewModels;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace FrontendTest;

public class ChartViewModelTests
{
    [Fact]
    public async Task SourceTab_Builds_Charts_With_Expected_Order_And_Colors()
    {
        var sources = new List<Source>
        {
            new()
            {
                Id = 1,
                FileName = "source.csv",
                TimeFrom = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                TimeTo = new DateTime(2026, 4, 1, 1, 0, 0, DateTimeKind.Utc),
                HeatDemand = 10,
                ElectricityPrice = 20
            },
            new()
            {
                Id = 2,
                FileName = "source.csv",
                TimeFrom = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                TimeTo = new DateTime(2026, 11, 1, 1, 0, 0, DateTimeKind.Utc),
                HeatDemand = 12,
                ElectricityPrice = 22
            }
        };

        var vm = new SourceTabViewModel(new SourceClient(CreateJsonClient(Array.Empty<Source>())));

        vm.Sources.Clear();
        foreach (var source in sources)
            vm.Sources.Add(source);

        InvokePrivate(vm, "BuildWinterSeries");
        InvokePrivate(vm, "BuildSummerSeries");

        Assert.Equal(2, vm.WinterSeries.Count);

        var firstSeries = Assert.IsType<LineSeries<DateTimePoint>>(vm.WinterSeries[0]);
        var secondSeries = Assert.IsType<LineSeries<DateTimePoint>>(vm.WinterSeries[1]);

        Assert.Equal(SKColor.Parse("#E4572E"), ((SolidColorPaint)firstSeries.Stroke!).Color);
        Assert.Equal(SKColor.Parse("#0084FF"), ((SolidColorPaint)secondSeries.Stroke!).Color);
        Assert.Equal(0, vm.DualAxes[0].MinLimit);
        Assert.Equal(0, vm.DualAxes[1].MinLimit);
    }

    [Fact]
    public async Task ResultsTab_Builds_Pie_Legend_And_Chart_Series()
    {
        var results = new List<OptimizedResults>
        {
            new()
            {
                Id = 1,
                Name = "Run 1",
                ResultsForHours =
                [
                    new ResultList
                    {
                        Id = 1,
                        TimeFrom = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                        TimeTo = new DateTime(2026, 4, 1, 1, 0, 0, DateTimeKind.Utc),
                        Results =
                        [
                            new Result
                            {
                                Id = 1,
                                AssetId = 1,
                                Asset = new Asset { Id = 1, Name = "Plant A" },
                                HeatProduction = 10,
                                Electricity = 2,
                                CO2Produced = 3,
                                ProductionCost = 4
                            },
                            new Result
                            {
                                Id = 2,
                                AssetId = 2,
                                Asset = new Asset { Id = 2, Name = "Plant B" },
                                HeatProduction = 20,
                                Electricity = 5,
                                CO2Produced = 6,
                                ProductionCost = 7
                            }
                        ]
                    },
                    new ResultList
                    {
                        Id = 2,
                        TimeFrom = new DateTime(2026, 4, 1, 1, 0, 0, DateTimeKind.Utc),
                        TimeTo = new DateTime(2026, 4, 1, 2, 0, 0, DateTimeKind.Utc),
                        Results =
                        [
                            new Result
                            {
                                Id = 3,
                                AssetId = 1,
                                Asset = new Asset { Id = 1, Name = "Plant A" },
                                HeatProduction = 10,
                                Electricity = 2,
                                CO2Produced = 3,
                                ProductionCost = 4
                            },
                            new Result
                            {
                                Id = 4,
                                AssetId = 2,
                                Asset = new Asset { Id = 2, Name = "Plant B" },
                                HeatProduction = 30,
                                Electricity = 7,
                                CO2Produced = 8,
                                ProductionCost = 9
                            }
                        ]
                    }
                ]
            }
        };

        var vm = new ResultsTabViewModel(new OptimizedResultsClient(CreateJsonClient(Array.Empty<OptimizedResults>())));

        InvokePrivate(vm, "RebuildCharts", vm.SelectedOptimizedResult?.ResultsForHours ?? results[0].ResultsForHours);
        InvokePrivate(vm, "RebuildGeneratorUsagePie", results[0].ResultsForHours);

        Assert.Equal(3, vm.HeatChartSeries.Length);
        var heatSeries = Assert.IsType<StackedAreaSeries<DateTimePoint>>(vm.HeatChartSeries[0]);
        var heatTotalLine = Assert.IsType<LineSeries<DateTimePoint>>(vm.HeatChartSeries[2]);
        var electricitySeries = Assert.IsType<LineSeries<DateTimePoint>>(vm.ElectricityChartSeries[0]);

        Assert.Equal(2, heatSeries.Values!.Count());
        Assert.Equal(2, electricitySeries.Values!.Count());
        Assert.Equal(2, vm.GeneratorUsageLegend.Count);
        Assert.Equal("Plant B", vm.GeneratorUsageLegend[0].Name);
        Assert.Equal("Plant A", vm.GeneratorUsageLegend[1].Name);
        Assert.Equal(2, vm.GeneratorUsagePieSeries.Length);

        var costSeries = Assert.IsType<StackedAreaSeries<DateTimePoint>>(vm.CostChartSeries[0]);
        var co2Series = Assert.IsType<StackedAreaSeries<DateTimePoint>>(vm.Co2ChartSeries[0]);
        var costTotalLine = Assert.IsType<LineSeries<DateTimePoint>>(vm.CostChartSeries[2]);
        var co2TotalLine = Assert.IsType<LineSeries<DateTimePoint>>(vm.Co2ChartSeries[2]);

        Assert.Equal(2, costSeries.Values!.Count());
        Assert.Equal(2, co2Series.Values!.Count());
        Assert.Equal(2, heatTotalLine.Values!.Count());
        Assert.Equal(2, costTotalLine.Values!.Count());
        Assert.Equal(2, co2TotalLine.Values!.Count());
    }

    private static HttpClient CreateJsonClient<T>(T payload)
    {
        var json = JsonSerializer.Serialize(payload);
        var handler = new FakeJsonHandler(json);
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8080")
        };
    }

    private static async Task WaitForConditionAsync(Func<bool> condition, int timeoutMs = 3000)
    {
        var start = Environment.TickCount64;
        while (!condition())
        {
            if (Environment.TickCount64 - start > timeoutMs)
                throw new TimeoutException("Condition was not met in time.");

            await Task.Delay(50);
        }
    }

    private static void InvokePrivate(object target, string methodName, params object[]? args)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (method == null)
            throw new InvalidOperationException($"Could not find method {methodName}.");

        method.Invoke(target, args);
    }

    private sealed class FakeJsonHandler : HttpMessageHandler
    {
        private readonly string _json;

        public FakeJsonHandler(string json)
        {
            _json = json;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_json, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }
}
