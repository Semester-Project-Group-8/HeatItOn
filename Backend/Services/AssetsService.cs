using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
namespace Backend.Services
{
    public class AssetsService:IService<Asset>
    {
        private readonly BackendDbContext _dbContext;
        public AssetsService(BackendDbContext dbContext)
        {
            _dbContext=dbContext;
        }

        public async Task<List<Asset>> List()
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
        

        public async Task<int> Post(int id, string name, float maxHeat, int productionCost, int co2Emission, float gasConsumption, float oilConsumption, float maxElectricity, string? imageName = null)
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
                ImageName = imageName
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
        

        public async Task<Asset> Get(int id)
        {
            var asset = await _dbContext.Assets.FindAsync(id);
            if (asset == null)
                throw new KeyNotFoundException($"Asset with ID {id} not found.");
            return asset;
        }

        public async Task Delete(int id)
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
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException($"Asset with ID {id} cannot be deleted due to existing dependencies.");
            }
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

            try
            {
                _dbContext.Assets.Update(asset);
                var result = await _dbContext.SaveChangesAsync();
                if (result <= 0)
                    throw new InvalidOperationException("Asset was not updated.");
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException($"Asset with ID {id} could not be updated due to a database error.");
            }
        }

    }
}
