using Backend.Data;
using Backend.Models;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace BackendTest;

public class ResultListServiceTests : IDisposable
{
    private readonly BackendDbContext _context;
    private readonly ResultListService _resultListService;

    public ResultListServiceTests()
    {
        var options = new DbContextOptionsBuilder<BackendDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BackendDbContext(options);
        _resultListService = new ResultListService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task ListResultLists_ShouldReturnEmpty_WhenNoItems()
    {
        var result = await _resultListService.ListResultLists();

        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateResultList_ShouldCreateResultList_WhenInputIsValid()
    {
        await SeedAsset(1, "GB1");

        var id = await _resultListService.CreateResultList(
            new DateTime(2026, 4, 10, 8, 0, 0),
            new DateTime(2026, 4, 10, 9, 0, 0),
            new List<Result>
            {
                new Result
                {
                    Id = 999,
                    HeatProduction = 2.5f,
                    Electricity = -0.8f,
                    ProductionCost = 1200f,
                    PrimaryEnergyConsumed = 3.1f,
                    CO2Produced = 95,
                    AssetId = 1
                }
            });

        Assert.True(id > 0);

        var saved = await _resultListService.GetResultList(id);
        Assert.Equal(new DateTime(2026, 4, 10, 8, 0, 0), saved.TimeFrom);
        Assert.Single(saved.Results);
        Assert.Equal(1, saved.Results.First().Id);
        Assert.Equal(1, saved.Results.First().AssetId);
    }

    [Fact]
    public async Task CreateResultList_ShouldThrow_WhenResultListIsEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _resultListService.CreateResultList(DateTime.UtcNow, DateTime.UtcNow.AddHours(1), new List<Result>()));
    }

    [Fact]
    public async Task CreateResultList_ShouldThrow_WhenSameAssetAppearsTwice()
    {
        await SeedAsset(1, "GB1");

        var payload = new List<Result>
        {
            new Result
            {
                HeatProduction = 1,
                Electricity = 0,
                ProductionCost = 10,
                PrimaryEnergyConsumed = 1,
                CO2Produced = 1,
                AssetId = 1
            },
            new Result
            {
                HeatProduction = 2,
                Electricity = 0,
                ProductionCost = 20,
                PrimaryEnergyConsumed = 2,
                CO2Produced = 2,
                AssetId = 1
            }
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _resultListService.CreateResultList(DateTime.UtcNow, DateTime.UtcNow.AddHours(1), payload));
    }

    [Fact]
    public async Task CreateResultList_ShouldThrow_WhenAssetDoesNotExist()
    {
        var payload = new List<Result>
        {
            new Result
            {
                HeatProduction = 1,
                Electricity = 0,
                ProductionCost = 10,
                PrimaryEnergyConsumed = 1,
                CO2Produced = 1,
                AssetId = 404
            }
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _resultListService.CreateResultList(DateTime.UtcNow, DateTime.UtcNow.AddHours(1), payload));
    }

    [Fact]
    public async Task DeleteResultList_ShouldDeleteListAndChildren()
    {
        await SeedAsset(1, "GB1");

        var id = await _resultListService.CreateResultList(
            new DateTime(2026, 4, 10, 8, 0, 0),
            new DateTime(2026, 4, 10, 9, 0, 0),
            new List<Result>
            {
                new Result
                {
                    HeatProduction = 2.5f,
                    Electricity = -0.8f,
                    ProductionCost = 1200f,
                    PrimaryEnergyConsumed = 3.1f,
                    CO2Produced = 95,
                    AssetId = 1
                }
            });

        var affected = await _resultListService.DeleteResultList(id);

        Assert.True(affected > 0);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _resultListService.GetResultList(id));
        Assert.Empty(_context.Results);
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
