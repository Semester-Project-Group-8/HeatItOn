using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.Services;

namespace BackendTest;

public class AssetServiceTests : IDisposable
{
    private readonly BackendDbContext _context;
    private readonly AssetsService _assetService;

    public AssetServiceTests()
    {
        var options = new DbContextOptionsBuilder<BackendDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BackendDbContext(options);
        _assetService = new AssetsService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    Asset test_asset = new Asset
    {
        Id = 1,
        Name = "Boiler",
        MaxHeat = 100f,
        ProductionCost = 5000,
        CO2Emission = 50,
        GasConsumption = 10f,
        OilConsumption = 5f,
        MaxElectricity = 50f
    };

    [Fact]
    public async Task AddAsset_PositiveCase()
    {
        await _assetService.Post(test_asset);

        var savedAsset = await _assetService.Get(1);

        Assert.NotNull(savedAsset);
        Assert.Equal("Boiler", savedAsset.Name);
    }

    [Fact]
    public async Task AddAsset_NegativeCase()
    {
         await _assetService.Post(test_asset);

        var savedAsset = await _assetService.Get(1);
        Assert.NotEqual("MashaBoss", savedAsset.Name);
    }

    [Fact]
    public async Task ListAssets_PositiveCase()
    {
        var test_asset1 = new Asset { Id = 1, Name = "Asset1", MaxHeat = 100, ProductionCost = 1000, CO2Emission = 10, GasConsumption = 1f, OilConsumption = 0.5f, MaxElectricity = 20f };
        var test_asset2 = new Asset { Id = 2, Name = "Asset2", MaxHeat = 200, ProductionCost = 2000, CO2Emission = 20, GasConsumption = 2f, OilConsumption = 1f, MaxElectricity = 40f };

        await _assetService.Post(test_asset1);
        await _assetService.Post(test_asset2);

        var result = await _assetService.List();

        Assert.Equal(2, result.Count());
    }
    
    [Fact]
    public async Task ListAssets_NegativeCase()
    {
        var test_asset1 = new Asset { Id = 1, Name = "Asset1", MaxHeat = 100, ProductionCost = 1000, CO2Emission = 10, GasConsumption = 1f, OilConsumption = 0.5f, MaxElectricity = 20f };
        var test_asset2 = new Asset { Id = 2, Name = "Asset2", MaxHeat = 200, ProductionCost = 2000, CO2Emission = 20, GasConsumption = 2f, OilConsumption = 1f, MaxElectricity = 40f };

        await _assetService.Post(test_asset1);
        await _assetService.Post(test_asset2);

        var result = await _assetService.List();

        Assert.NotEqual(5, result.Count());
    }

    [Fact]
    public async Task GetExistingAsset_PositiveCase()
    {
        await _assetService.Post(test_asset);

        var result = await _assetService.Get(1);

        Assert.Equal("Boiler", result.Name);
        Assert.Equal(100, result.MaxHeat);
    }

    [Fact]
    public async Task GetExistingAsset_NegativeCase()
    {
        await _assetService.Post(test_asset);

        var result = await _assetService.Get(1);

        Assert.NotEqual("Test Asset1111", result.Name);
        Assert.NotEqual(1000, result.MaxHeat);
    }

    [Fact]
    public async Task GetExistingAsset_EdgeCase() {

        await Assert.ThrowsAsync<KeyNotFoundException>(async () => 
            await _assetService.Get(1));
    }

    [Fact]
    public async Task UpdateAsset_PositiveCase()
    {
        await _assetService.Post(test_asset);

        await _assetService.Put(1, new Asset { Name = "New Name", MaxHeat = 150, ProductionCost = 6000, CO2Emission = 60, GasConsumption = 12, OilConsumption = 6, MaxElectricity = 55 });

        var updatedAsset = await _assetService.Get(1);
        Assert.Equal("New Name", updatedAsset.Name);
        Assert.Equal(150, updatedAsset.MaxHeat);
    }

    [Fact]
    public async Task UpdateAsset_NegativeCase()
    {
        await _assetService.Post(test_asset);

        await _assetService.Put(1, new Asset { Name = "New Name", MaxHeat = 150, ProductionCost = 6000, CO2Emission = 60, GasConsumption = 12, OilConsumption = 6, MaxElectricity = 55 });

        var updatedAsset = await _assetService.Get(1);
        Assert.NotEqual("Boiler", updatedAsset.Name);
        Assert.NotEqual(100, updatedAsset.MaxHeat);
    }

    [Fact]
    public async Task UpdateNonExistentAsset_EdgeCase()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _assetService.Put(999, new Asset { Name = "Name", MaxHeat = 100, ProductionCost = 5000, CO2Emission = 50, GasConsumption = 10, OilConsumption = 5, MaxElectricity = 50 }));
    }

    [Fact]
    public async Task DeleteExistingAsset_PositiveCase()
    {
        await _assetService.Post(test_asset);

        await _assetService.Delete(1);

        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _assetService.Get(1));
    }

    [Fact]
    public async Task DeleteNonExistentAsset_EdgeCase()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _assetService.Delete(999));
    }

 }
