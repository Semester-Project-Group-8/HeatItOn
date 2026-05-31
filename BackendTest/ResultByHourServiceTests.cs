using Backend.Data;
using Backend.Models;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace BackendTest;

public class ResultListServiceTests : IDisposable
{
    private readonly BackendDbContext _context;
    private readonly ResultByHourService _resultListService;

    public ResultListServiceTests()
    {
        var options = new DbContextOptionsBuilder<BackendDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BackendDbContext(options);
        _resultListService = new ResultByHourService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task List_ShouldReturnNumberOfItems()
    {
        await SeedAsset(1, "GB1");

        await _resultListService.PostList(CreateResultLists());

        var result = await _resultListService.List();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task List_ShouldReturnEmpty_WhenNoItems()
    {
        var result = await _resultListService.List();

        Assert.Empty(result);
    }

    [Fact]
    public async Task PostList_ShouldAddResultLists_WhenInputIsValid()
    {
        await SeedAsset(1, "GB1");

        await _resultListService.PostList(CreateResultLists());

        var saved = await _resultListService.List();
        Assert.Equal(2, saved.Count);
        Assert.Equal(new DateTime(2026, 4, 10, 8, 0, 0), saved.First().TimeFrom);
        Assert.Equal(2, saved.First().Results.Count);
        Assert.Equal(1, saved.First().Results.First().AssetId);
    }

    [Fact]
    public async Task Post_ShouldAddSingleResultList_WhenInputIsValid()
    {
        await SeedAsset(1, "GB1");

        var resultList = CreateResultList(
            new DateTime(2026, 4, 10, 8, 0, 0),
            new DateTime(2026, 4, 10, 9, 0, 0),
            1,
            2.5f,
            -0.8f,
            1200f,
            3.1f,
            95);

        await _resultListService.Post(resultList);

        var saved = await _resultListService.List();
        Assert.Single(saved);
        Assert.Equal(new DateTime(2026, 4, 10, 8, 0, 0), saved.First().TimeFrom);
        Assert.Single(saved.First().Results);
    }

    [Fact]
    public async Task Put_ShouldUpdateResultList_WhenInputIsValid()
    {
        await SeedAsset(1, "GB1");
        await _resultListService.PostList(CreateResultLists());

        var existing = await _resultListService.List();
        var id = existing.First().Id;

        var updated = CreateResultList(
            new DateTime(2026, 4, 10, 12, 0, 0),
            new DateTime(2026, 4, 10, 13, 0, 0),
            id,
            5f,
            -0.9f,
            67f,
            0.8f,
            650);
        updated.Id = id;

        await _resultListService.Put(id, updated);

        var saved = await _resultListService.Get(id);
        Assert.Equal(new DateTime(2026, 4, 10, 12, 0, 0), saved.TimeFrom);
        Assert.Equal(new DateTime(2026, 4, 10, 13, 0, 0), saved.TimeTo);
        Assert.Single(saved.Results);
        Assert.Equal(650, saved.Results.First().CO2Produced);
    }

    [Fact]
    public async Task Delete_ShouldDeleteListAndChildren()
    {
        await SeedAsset(1, "GB1");
        await _resultListService.PostList(CreateResultLists());

        var existing = await _resultListService.List();
        var id = existing.First().Id;

        await _resultListService.Delete(id);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _resultListService.Get(id));
        Assert.Single(await _resultListService.List());
    }

    private static List<ResultByHour> CreateResultLists()
    {
        return new List<ResultByHour>
        {
            CreateResultList(
                new DateTime(2026, 4, 10, 8, 0, 0),
                new DateTime(2026, 4, 10, 9, 0, 0),
                1,
                2.5f,
                -0.8f,
                1200f,
                3.1f,
                95,
                2.5f,
                8f,
                8750f,
                6.7f,
                950),
            CreateResultList(
                new DateTime(2026, 4, 10, 10, 0, 0),
                new DateTime(2026, 4, 10, 11, 0, 0),
                1,
                5f,
                -0.9f,
                120f,
                1f,
                78,
                2f,
                88f,
                67f,
                0.8f,
                650)
        };
    }

    private static ResultByHour CreateResultList(
        DateTime timeFrom,
        DateTime timeTo,
        int assetId,
        float firstHeatProduction,
        float firstElectricity,
        float firstProductionCost,
        float firstPrimaryEnergyConsumed,
        int firstCO2Produced,
        float secondHeatProduction = 0,
        float secondElectricity = 0,
        float secondProductionCost = 0,
        float secondPrimaryEnergyConsumed = 0,
        int secondCO2Produced = 0)
    {
        var results = new List<Result>
        {
            new Result
            {
                HeatProduction = firstHeatProduction,
                Electricity = firstElectricity,
                ProductionCost = firstProductionCost,
                PrimaryEnergyConsumed = firstPrimaryEnergyConsumed,
                CO2Produced = firstCO2Produced,
                AssetId = assetId
            }
        };

        if (secondCO2Produced != 0 || secondProductionCost != 0 || secondPrimaryEnergyConsumed != 0 || secondElectricity != 0 || secondHeatProduction != 0)
        {
            results.Add(new Result
            {
                HeatProduction = secondHeatProduction,
                Electricity = secondElectricity,
                ProductionCost = secondProductionCost,
                PrimaryEnergyConsumed = secondPrimaryEnergyConsumed,
                CO2Produced = secondCO2Produced,
                AssetId = assetId
            });
        }

        return new ResultByHour
        {
            TimeFrom = timeFrom,
            TimeTo = timeTo,
            Results = results
        };
    }

    private async Task SeedAsset(int id, string name)
    {
        _context.Assets.Add(new Asset
        {
            Id = id,
            Name = name,
            MaxHeat = 3,
            ProductionCost = 510,
            CO2Emission = 132,
            GasConsumption = 1.05f,
            OilConsumption = 0,
            MaxElectricity = 0
        });

        await _context.SaveChangesAsync();
    }
}
