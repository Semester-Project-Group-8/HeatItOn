using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

namespace Backend.Services;

public class AssetsService : IService<Asset>
{
    private readonly BackendDbContext _dbContext;

    public AssetsService(BackendDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Post(Asset value)
    {
        var asset = await _dbContext.Assets.AddAsync(value);
        var result = await _dbContext.SaveChangesAsync();
    }

    public async Task Put(int id, Asset value)
    {
        var asset = await _dbContext.Assets.FindAsync(id);
        if (asset == null)
            throw new KeyNotFoundException($"Asset with ID {id} not found.");

        asset.Name = value.Name;
        asset.MaxHeat = value.MaxHeat;
        asset.ProductionCost = value.ProductionCost;
        asset.CO2Emission = value.CO2Emission;
        asset.GasConsumption = value.GasConsumption;
        asset.OilConsumption = value.OilConsumption;
        asset.MaxElectricity = value.MaxElectricity;
        asset.ImageName = value.ImageName;

        _dbContext.Assets.Update(asset);
        var result = await _dbContext.SaveChangesAsync();
    }

    public async Task<List<Asset>> List()
    {
        return await _dbContext.Assets.ToListAsync();
    }

    public async Task<Asset> Get(int id)
    {
        var asset = await _dbContext.Assets.FindAsync(id);
        return asset ?? throw new KeyNotFoundException($"Asset with ID {id} not found.");
    }

    public async Task Delete(int id)
    {
        var asset = await _dbContext.Assets.FindAsync(id);
        if (asset == null) throw new KeyNotFoundException($"Asset with ID {id} not found.");

        _dbContext.Assets.Remove(asset);
        await _dbContext.SaveChangesAsync();
    }
}