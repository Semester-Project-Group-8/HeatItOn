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

    [Fact]
    public async Task AddAsset_PositiveCase()
    {
        var result = await _assetService.AddAsset(
            id: 1,
            name: "Boiler",
            maxHeat: 100f,
            productionCost: 5000,
            co2Emission: 50,
            gasConsumption: 10f,
            oilConsumption: 5f,
            maxElectricity: 50f
        );

        Assert.Equal(1, result);
        var savedAsset = await _assetService.GetAsset(1);
        Assert.Equal("Boiler", savedAsset.Name);
    }

    [Fact]
    public async Task AddAsset_NegativeCase()
    {
        var result = await _assetService.AddAsset(
            id: 1,
            name: "Boiler",
            maxHeat: 100f,
            productionCost: 5000,
            co2Emission: 50,
            gasConsumption: 10f,
            oilConsumption: 5f,
            maxElectricity: 50f
        );

        Assert.NotEqual(3, result);
        var savedAsset = await _assetService.GetAsset(1);
        Assert.NotEqual("MashaBoss", savedAsset.Name);
    }

    // [Fact]
    // public async Task AddAsset_EdgeCase()
    // {
    //     var image = new Image { Id = 1, ImageLink = "test.png" };
    //     _context.Images.Add(image);
    //     await _context.SaveChangesAsync();

    //     await Assert.ThrowsAsync<ArgumentNullException>(async () =>
    //         await _assetService.AddAsset(
    //             id: 1,
    //             name: null!,
    //             maxHeat: 0,
    //             productionCost: 0,
    //             co2Emission: 0,
    //             gasConsumption: 0,
    //             oilConsumption: 0,
    //             maxElectricity: 0f,
    //             imageId: 1,
    //             image: image));
    // }

    [Fact]
    public async Task ListAssets_PositiveCase()
    {
        var assets = new List<Asset>
        {
            new Asset { Id = 1, Name = "Asset1", MaxHeat = 100, ProductionCost = 1000, CO2Emission = 10, GasConsumption = 1f, OilConsumption = 0.5f, MaxElectricity = 20f },
            new Asset { Id = 2, Name = "Asset2", MaxHeat = 200, ProductionCost = 2000, CO2Emission = 20, GasConsumption = 2f, OilConsumption = 1f, MaxElectricity = 40f }
        };
        await _assetService.AddAssets(assets);

        var result = await _assetService.ListAssets();

        Assert.Equal(2, result.Count());
    }
    
    [Fact]
    public async Task ListAssets_NegativeCase()
    {
        var image = new Image { Id = 1, ImageLink = "test.png" };
        _context.Images.Add(image);
        await _context.SaveChangesAsync();

        var assets = new List<Asset>
        {
            new Asset { Id = 1, Name = "Asset1", MaxHeat = 100, ProductionCost = 1000, CO2Emission = 10, GasConsumption = 1f, OilConsumption = 0.5f, MaxElectricity = 20f }, //ImageId = 1, Image = image },
            new Asset { Id = 2, Name = "Asset2", MaxHeat = 200, ProductionCost = 2000, CO2Emission = 20, GasConsumption = 2f, OilConsumption = 1f, MaxElectricity = 40f }//, ImageId = 1, Image = image }
        };
        await _assetService.AddAssets(assets);

        var result = await _assetService.ListAssets();

        Assert.NotEqual(5, result.Count());
    }

    [Fact]
    public async Task GetExistingAsset_PositiveCase()
    {
        await _assetService.AddAsset(1, "Test Asset", 100, 5000, 50, 10, 5, 50);

        var result = await _assetService.GetAsset(1);

        Assert.NotNull(result);
        Assert.Equal("Test Asset", result.Name);
        Assert.Equal(100, result.MaxHeat);
    }

    [Fact]
    public async Task GetExistingAsset_NegativeCase()
    {
        await _assetService.AddAsset(1, "Test Asset", 100, 5000, 50, 10, 5, 50);

        var result = await _assetService.GetAsset(1);

        Assert.NotEqual("Test Asset1111", result.Name);
        Assert.NotEqual(1000, result.MaxHeat);
    }

    [Fact]
    public async Task UpdateAsset_PositiveCase()
    {
        await _assetService.AddAsset(1, "Old Name", 100, 5000, 50, 10, 5, 50);

        var result = await _assetService.UpdateAsset(1, "New Name", 150, 6000, 60, 12, 6, 55);

        Assert.Equal(1, result);
        var updatedAsset = await _assetService.GetAsset(1);
        Assert.Equal("New Name", updatedAsset.Name);
        Assert.Equal(150, updatedAsset.MaxHeat);
    }

    [Fact]
    public async Task UpdateAsset_NegativeCase()
    {
        await _assetService.AddAsset(1, "Old Name", 100, 5000, 50, 10, 5, 50);

        var result = await _assetService.UpdateAsset(1, "New Name", 150, 6000, 60, 12, 6, 55);

        Assert.NotEqual(2, result);
        var updatedAsset = await _assetService.GetAsset(1);
        Assert.NotEqual("Masha star", updatedAsset.Name);
        Assert.NotEqual(1, updatedAsset.MaxHeat);
    }

    [Fact]
    public async Task UpdateNonExistentAsset_EdgeCase()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _assetService.UpdateAsset(999, "Name", 100, 5000, 50, 10, 5, 50));
    }

    [Fact]
    public async Task DeleteExistingAsset_PositiveCase()
    {
        await _assetService.AddAsset(1, "Test", 100, 5000, 50, 10, 5, 50);

        await _assetService.DeleteAsset(1);

        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _assetService.GetAsset(1));
    }

    [Fact]
    public async Task DeleteNonExistentAsset_EdgeCase()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _assetService.DeleteAsset(999));
    }

    [Fact]
    public async Task AddAssets_PositiveCase()
    {
        var image = new Image { Id = 1, ImageLink = "test.png" };
        _context.Images.Add(image);
        await _context.SaveChangesAsync();

        var assets = new List<Asset>
        {
            new Asset { Id = 1, Name = "Asset1", MaxHeat = 100, ProductionCost = 1000, CO2Emission = 10, GasConsumption = 1f, OilConsumption = 0.5f, MaxElectricity = 20f }, //ImageId = 1, Image = image },
            new Asset { Id = 2, Name = "Asset2", MaxHeat = 200, ProductionCost = 2000, CO2Emission = 20, GasConsumption = 2f, OilConsumption = 1f, MaxElectricity = 40f }//, ImageId = 1, Image = image }
        };

        var result = await _assetService.AddAssets(assets);

        Assert.Equal(2, result);
        var allAssets = await _assetService.ListAssets();
        Assert.Equal(2, allAssets.Count());
    }

    [Fact]
    public async Task AddAssets_NegativeCase()
    {
        var image = new Image { Id = 1, ImageLink = "test.png" };
        _context.Images.Add(image);
        await _context.SaveChangesAsync();

        var assets = new List<Asset>
        {
            new Asset { Id = 1, Name = "Asset1", MaxHeat = 100, ProductionCost = 1000, CO2Emission = 10, GasConsumption = 1f, OilConsumption = 0.5f, MaxElectricity = 20f }, //ImageId = 1, Image = image },
            new Asset { Id = 2, Name = "Asset2", MaxHeat = 200, ProductionCost = 2000, CO2Emission = 20, GasConsumption = 2f, OilConsumption = 1f, MaxElectricity = 40f }//, ImageId = 1, Image = image }
        };

        var result = await _assetService.AddAssets(assets);

        Assert.NotEqual(6, result);
        var allAssets = await _assetService.ListAssets();
        Assert.NotEqual(1, allAssets.Count());
    }


    [Fact]
    public async Task AddAssets_EmptyList_EdgeCase()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () => await _assetService.AddAssets(new List<Asset>()));
    }

    [Fact]
    public async Task AddAssets_NullList_EdgeCase()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () => await _assetService.AddAssets(null!));
    }
}
