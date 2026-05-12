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

        public async Task Post(Asset asset)
        {
            await _dbContext.Assets.AddAsync(asset);
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
        


    }
}
