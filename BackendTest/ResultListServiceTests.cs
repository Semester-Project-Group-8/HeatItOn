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

    List<ResultByHour> resultLists = new List<ResultByHour>
        {
            new ResultByHour
            {
                TimeFrom = new DateTime(2026, 4, 10, 8, 0, 0),
                TimeTo = new DateTime(2026, 4, 10, 9, 0, 0),
                Results = new List<Result>
                {
                    new Result
                    {
                        HeatProduction = 2.5f,
                        Electricity = -0.8f,
                        ProductionCost = 1200f,
                        PrimaryEnergyConsumed = 3.1f,
                        CO2Produced = 95,
                        AssetId = 1
                    },
                    new Result
                    {
                        HeatProduction = 2.5f,
                        Electricity = 8f,
                        ProductionCost = 8750f,
                        PrimaryEnergyConsumed = 6.7f,
                        CO2Produced = 950,
                        AssetId = 1
                    }
                }
            },
            new ResultByHour
            {
                TimeFrom = new DateTime(2026, 4, 10, 10, 0, 0),
                TimeTo = new DateTime(2026, 4, 10, 11, 0, 0),
                Results = new List<Result>
                {
                    new Result
                    {
                        HeatProduction = 5f,
                        Electricity = -0.9f,
                        ProductionCost = 120f,
                        PrimaryEnergyConsumed = 1f,
                        CO2Produced = 78,
                        AssetId = 1
                    },
                    new Result
                    {
                        HeatProduction = 2f,
                        Electricity = 88f,
                        ProductionCost = 67f,
                        PrimaryEnergyConsumed = 0.8f,
                        CO2Produced = 650,
                        AssetId = 1
                    }
                }
            }
        };

        List<Result> result = new List<Result>
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

    [Fact]
    public async Task ListResultLists_ShouldReturnNumberOfItems()
    {
        var savedCount = await _resultListService.Post(resultLists);
        var result = await _resultListService.ListResultLists();

        Assert.Equal(2, result.Count());
    }


    [Fact]
    public async Task ListResultLists_ShouldReturnEmpty_WhenNoItems()
    {
        var result = await _resultListService.ListResultLists();

        Assert.Empty(result);
    }

    [Fact]
    public async Task AddResultList_ShouldAddResultList_WhenInputIsValid()
    {
        await SeedAsset(1, "GB1");
        var savedCount = await _resultListService.Post(resultLists);

        Assert.True(savedCount > 0);

        var saved = await _resultListService.ListResultLists();
        Assert.Equal(2, saved.Count());
        Assert.Equal(new DateTime(2026, 4, 10, 8, 0, 0), saved.First().TimeFrom);
        Assert.Equal(2, saved.First().Results.Count);
        Assert.Equal(1, saved.First().Results.First().AssetId);
    }

    [Fact]
    public async Task CreateResultList_ShouldThrow_WhenResultListIsEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _resultListService.Put(DateTime.UtcNow, DateTime.UtcNow.AddHours(1), new List<Result>()));
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
            _resultListService.Put(DateTime.Now, DateTime.Now.AddHours(1), payload));
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
            _resultListService.Put(DateTime.Now, DateTime.Now.AddHours(1), payload));
    }

    [Fact]
    public async Task CreateResultList_ShouldCreate_WhenInputIsValid()
    {
        await SeedAsset(1, "GB1");

        var payload = new List<Result>
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
        };

        var id = await _resultListService.Put(
            new DateTime(2026, 4, 10, 8, 0, 0),
            new DateTime(2026, 4, 10, 9, 0, 0),
            payload);

        Assert.True(id > 0);

        var saved = await _resultListService.Get(id);
        Assert.Equal(new DateTime(2026, 4, 10, 8, 0, 0), saved.TimeFrom);
        Assert.Equal(new DateTime(2026, 4, 10, 9, 0, 0), saved.TimeTo);
        Assert.Single(saved.Results);
        Assert.Equal(1, saved.Results.First().AssetId);
    }

    [Fact]
    public async Task DeleteResultList_ShouldDeleteListAndChildren()
    {
        await SeedAsset(1, "GB1");

        var id = await _resultListService.Put(
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
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _resultListService.Get(id));
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
