using System.Net;
using System.Text;
using System.Text.Json;
using Frontend.Data;
using Frontend.Models;
using Frontend.ViewModels;

namespace FrontendTest;

public class ResultsTabViewModelTests
{
    private static ResultsTabViewModel CreateViewModel()
    {
        var json = JsonSerializer.Serialize(new List<OptimizedResults>());
        var handler = new StubHttpMessageHandler((request, _) =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        return new ResultsTabViewModel(new OptimizedResultsClient(httpClient));
    }

    private static OptimizedResults MakeResult(params (DateTime time, float heat, float elec, int co2, float cost)[] hours)
    {
        return new OptimizedResults
        {
            Id = 1,
            Name = "Test",
            ResultsForHours = hours.Select(h => new ResultList
            {
                TimeFrom = h.time,
                TimeTo = h.time.AddHours(1),
                Results =
                [
                    new Result
                    {
                        HeatProduction = h.heat,
                        Electricity = h.elec,
                        CO2Produced = h.co2,
                        ProductionCost = h.cost,
                        Asset = new Asset { Name = "GB1" }
                    }
                ]
            }).ToList()
        };
    }

    [Fact]
    public void TotalHeatDisplay_SumsHeatFromAllRows()
    {
        var vm = CreateViewModel();
        var t = new DateTime(2025, 1, 1, 0, 0, 0);

        vm.SelectedOptimizedResult = MakeResult(
            (t, 3.5f, 0, 0, 0),
            (t.AddHours(1), 2.0f, 0, 0, 0)
        );

        Assert.Equal("5.5 MW", vm.TotalHeatDisplay);
    }

    [Fact]
    public void NetElectricityDisplay_PositiveSum_HasPlusSign()
    {
        var vm = CreateViewModel();
        var t = new DateTime(2025, 1, 1, 0, 0, 0);

        vm.SelectedOptimizedResult = MakeResult(
            (t, 0, 4.0f, 0, 0),
            (t.AddHours(1), 0, 2.5f, 0, 0)
        );

        Assert.StartsWith("+", vm.NetElectricityDisplay);
        Assert.Contains("6.5", vm.NetElectricityDisplay);
        Assert.EndsWith("MW", vm.NetElectricityDisplay);
    }

    [Fact]
    public void NetElectricityDisplay_NegativeSum_HasNoPlus()
    {
        var vm = CreateViewModel();
        var t = new DateTime(2025, 1, 1, 0, 0, 0);

        vm.SelectedOptimizedResult = MakeResult(
            (t, 0, -3.0f, 0, 0)
        );

        Assert.StartsWith("-", vm.NetElectricityDisplay);
        Assert.EndsWith("MW", vm.NetElectricityDisplay);
    }

    [Fact]
    public void TotalCo2Display_SumsCo2WithKgSuffix()
    {
        var vm = CreateViewModel();
        var t = new DateTime(2025, 1, 1, 0, 0, 0);

        vm.SelectedOptimizedResult = MakeResult(
            (t, 0, 0, 100, 0),
            (t.AddHours(1), 0, 0, 50, 0)
        );

        Assert.EndsWith("KG", vm.TotalCo2Display);
        Assert.Contains("150", vm.TotalCo2Display);
    }

    [Fact]
    public void TotalCostDisplay_SmallCost_FormatsAsDkk()
    {
        var vm = CreateViewModel();
        var t = new DateTime(2025, 1, 1, 0, 0, 0);

        vm.SelectedOptimizedResult = MakeResult(
            (t, 0, 0, 0, 400f),
            (t.AddHours(1), 0, 0, 0, 200f)
        );

        Assert.EndsWith("DKK", vm.TotalCostDisplay);
        Assert.DoesNotContain("K DKK", vm.TotalCostDisplay);
        Assert.Contains("600", vm.TotalCostDisplay);
    }

    [Fact]
    public void TotalCostDisplay_LargeCost_FormatsAsKDkk()
    {
        var vm = CreateViewModel();
        var t = new DateTime(2025, 1, 1, 0, 0, 0);

        vm.SelectedOptimizedResult = MakeResult(
            (t, 0, 0, 0, 5000f)
        );

        Assert.EndsWith("K DKK", vm.TotalCostDisplay);
        Assert.Contains("5", vm.TotalCostDisplay);
    }

    [Fact]
    public void Displays_AreEmpty_WhenNoResultSelected()
    {
        var vm = CreateViewModel();

        Assert.Equal("0.0 MW", vm.TotalHeatDisplay);
        Assert.Equal("+0.0 MW", vm.NetElectricityDisplay);
        Assert.EndsWith("KG", vm.TotalCo2Display);
        Assert.EndsWith("DKK", vm.TotalCostDisplay);
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> handler)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(handler(request, cancellationToken));
    }
}
