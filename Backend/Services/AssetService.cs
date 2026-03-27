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
            try
            {
                return await _dbContext.Assets.ToListAsync();
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Assets could not be loaded.");
            }
        }

        public async Task<int> AddAssets(List<Asset> assets)
        {
            if (assets == null || assets.Count == 0)
                throw new ArgumentException("No assets sent.");
            try
            {
                await _dbContext.Assets.AddRangeAsync(assets);
                var result = await _dbContext.SaveChangesAsync();
                if (result <= 0)
                    throw new InvalidOperationException("Assets were not saved.");

                return result;
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException("Assets could not be saved due to a database error.");
            }
        }
        
        public async Task<int> AddAsset(int id, string name, float maxHeat, int productionCost, int co2Emission, float gasConsumption, float oilConsumption, float maxElectricity)
        {
            var exists = await _dbContext.Assets.AnyAsync(a => a.Id == id);
            if (exists)
                throw new InvalidOperationException($"Asset with ID {id} already exists.");

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
            try
            {
                await _dbContext.Assets.AddAsync(asset);
                var savedRows = await _dbContext.SaveChangesAsync();

                if (savedRows <= 0)
                    throw new InvalidOperationException("Asset was not saved.");

                return savedRows;
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException($"Asset with ID {id} already exists.");
            }
        }

        public async Task<int> DeleteAsset(int id)
        {
            var asset = await _dbContext.Assets.FindAsync(id);
            if (asset == null)
                throw new KeyNotFoundException($"Asset with ID {id} not found.");
            try 
            {
                _dbContext.Assets.Remove(asset);
                var result = await _dbContext.SaveChangesAsync();
                if (result <= 0)
                    throw new InvalidOperationException("Asset was not deleted.");  

                return result;
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException($"Asset with ID {id} cannot be deleted due to existing dependencies.");
            }
        }
        
        public async Task<int> UpdateAsset(int id, string name, float maxHeat, int productionCost, int co2Emission, float gasConsumption, float oilConsumption, float maxElectricity)
        {
            var asset = await _dbContext.Assets.FindAsync(id);
            if (asset == null)
                throw new KeyNotFoundException($"Asset with ID {id} not found.");

            asset.Name = name;
            asset.MaxHeat = maxHeat;
            asset.ProductionCost = productionCost;
            asset.CO2Emission = co2Emission;
            asset.GasConsumption = gasConsumption;
            asset.OilConsumption = oilConsumption;
            asset.MaxElectricity = maxElectricity;

            try 
            {
                _dbContext.Assets.Update(asset);
                var result = await _dbContext.SaveChangesAsync();
                if (result <= 0)
                    throw new InvalidOperationException("Asset was not updated.");  
                    
                return result;
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException($"Asset with ID {id} could not be updated due to a database error.");
            }
        }

        public async Task<Asset> GetAsset(int id)
        {
            var asset = await _dbContext.Assets.FindAsync(id);
            if (asset == null)
                throw new KeyNotFoundException($"Asset with ID {id} not found.");
            return asset;
        }
    
    }
}