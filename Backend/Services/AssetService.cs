using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
namespace Backend.Services
{
    public class AssetsService
    {
        private readonly BackendDbContext _dbContext;
        public AssetsService(BackendDbContext dbContext)
        {
            _dbContext=dbContext;
        }

        public async Task<IEnumerable<Asset>> ListAssets() 
        {
            return await _dbContext.Assets.ToListAsync();
        }

        public async Task<int> AddAssets(List<Asset> assets)
        {
            if (assets == null || assets.Count == 0)
                throw new ArgumentException("No assets sent.");
            await _dbContext.Assets.AddRangeAsync(assets);
            return await _dbContext.SaveChangesAsync();
        }
        
        public async Task<int> AddAsset(int id, string name, float maxHeat, int productionCost, int co2Emission, float gasConsumption, float oilConsumption, float maxElectricity)
        {
            Asset asset = new Asset
            {
                Id= id,
                Name = name,
                MaxHeat= maxHeat,
                ProductionCost= productionCost, 
                CO2Emission= co2Emission,
                GasConsumption= gasConsumption,
                OilConsumption= oilConsumption,
                MaxElectricity= maxElectricity,
            };
            await _dbContext.Assets.AddAsync(asset);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsset(int id)
        {
            var asset = await _dbContext.Assets.FindAsync(id)
                ?? throw new KeyNotFoundException($"Asset with ID {id} not found.");
            _dbContext.Assets.Remove(asset);
            return await _dbContext.SaveChangesAsync();
        }
        
        public async Task<int> UpdateAsset(int id, string name, float maxHeat, int productionCost, int co2Emission, float gasConsumption, float oilConsumption, float maxElectricity)
        {
            var asset = await _dbContext.Assets.FindAsync(id)
                ?? throw new KeyNotFoundException($"Asset with ID {id} not found.");
            asset.Name = name;
            asset.MaxHeat = maxHeat;
            asset.ProductionCost = productionCost;
            asset.CO2Emission = co2Emission;
            asset.GasConsumption = gasConsumption;
            asset.OilConsumption = oilConsumption;
            asset.MaxElectricity = maxElectricity;

            _dbContext.Assets.Update(asset);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<Asset> GetAsset(int id)
        {
            return await _dbContext.Assets.FindAsync(id)
                ?? throw new KeyNotFoundException($"Asset with ID {id} not found.");
        }
    
    }
}