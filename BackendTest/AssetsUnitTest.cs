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
    public async Task AddAsset()
    {
        var image = new Image { Id = 1, ImageLink = "test.png" };
        _context.Images.Add(image);
        await _context.SaveChangesAsync();

        var result = await _assetService.AddAsset(
            id: 1,
            name: "Boiler",
            maxHeat: 100f,
            productionCost: 5000,
            co2Emission: 50,
            gasConsumption: 10f,
            oilConsumption: 5f,
            maxElectricity: 50f,
            imageId: 1,
            image: image
        );

        Assert.Equal(1, result);
        var savedAsset = await _assetService.GetAsset(1);
        Assert.Equal("Boiler", savedAsset.Name);
    }

    [Fact]
    public async Task AddAsset_NegativeCase()
    {
        var image = new Image { Id = 1, ImageLink = "test.png" };
        _context.Images.Add(image);
        await _context.SaveChangesAsync();

        var result = await _assetService.AddAsset(
            id: 1,
            name: "Boiler",
            maxHeat: 100f,
            productionCost: 5000,
            co2Emission: 50,
            gasConsumption: 10f,
            oilConsumption: 5f,
            maxElectricity: 50f,
            imageId: 1,
            image: image
        );

        Assert.NotEqual(3, result);
        var savedAsset = await _assetService.GetAsset(1);
        Assert.NotEqual("MashaBoss", savedAsset.Name);
    }

    [Fact]
    public async Task ListAssets_Multiple()
    {
        var image = new Image { Id = 1, ImageLink = "test.png" };
        _context.Images.Add(image);
        await _context.SaveChangesAsync();

        var assets = new List<Asset>
        {
            new Asset { Id = 1, Name = "Asset1", MaxHeat = 100, ProductionCost = 1000, CO2Emission = 10, GasConsumption = 1f, OilConsumption = 0.5f, MaxElectricity = 20f, ImageId = 1, Image = image },
            new Asset { Id = 2, Name = "Asset2", MaxHeat = 200, ProductionCost = 2000, CO2Emission = 20, GasConsumption = 2f, OilConsumption = 1f, MaxElectricity = 40f, ImageId = 1, Image = image }
        };
        await _assetService.AddAssets(assets);

        var result = await _assetService.ListAssets();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetExistingAsset()
    {
        var image = new Image { Id = 1, ImageLink = "test.png" };
        _context.Images.Add(image);
        await _assetService.AddAsset(1, "Test Asset", 100, 5000, 50, 10, 5, 50, 1, image);

        var result = await _assetService.GetAsset(1);

        Assert.NotNull(result);
        Assert.Equal("Test Asset", result.Name);
        Assert.Equal(100, result.MaxHeat);
    }

    [Fact]
    public async Task UpdateAsset()
    {
        var image = new Image { Id = 1, ImageLink = "test.png" };
        _context.Images.Add(image);
        await _assetService.AddAsset(1, "Old Name", 100, 5000, 50, 10, 5, 50, 1, image);

        var result = await _assetService.UpdateAsset(1, "New Name", 150, 6000, 60, 12, 6, 55, 1, image);

        Assert.Equal(1, result);
        var updatedAsset = await _assetService.GetAsset(1);
        Assert.Equal("New Name", updatedAsset.Name);
        Assert.Equal(150, updatedAsset.MaxHeat);
    }

    [Fact]
    public async Task UpdateNonExistentAsset()
    {
        var image = new Image { Id = 1, ImageLink = "test.png" };

        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _assetService.UpdateAsset(999, "Name", 100, 5000, 50, 10, 5, 50, 1, image));
    }

    [Fact]
    public async Task DeleteExistingAsset()
    {
        var image = new Image { Id = 1, ImageLink = "test.png" };
        _context.Images.Add(image);
        await _assetService.AddAsset(1, "Test", 100, 5000, 50, 10, 5, 50, 1, image);

        await _assetService.DeleteAsset(1);

        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _assetService.GetAsset(1));
    }

    [Fact]
    public async Task DeleteNonExistentAsset()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _assetService.DeleteAsset(999));
    }

    [Fact]
    public async Task AddAssets_EmptyList()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () => await _assetService.AddAssets(new List<Asset>()));
    }

    [Fact]
    public async Task AddAssets_NullList()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () => await _assetService.AddAssets(null!));
    }
}
