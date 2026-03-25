using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
namespace Backend.Services
{
	public class ResultService
	{
		private readonly BackendDbContext _dbContext;

		// ResultService 
		public ResultService(BackendDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<IEnumerable<Result>> ListResult()
		{
			return await _dbContext.Results.ToListAsync();
		}

		//AddResult ( for the list of the results )
		public async Task<int> AddResult(List<Result> result)
		{
			if (result == null || result.Count == 0)
			{
				Console.WriteLine("Error | No results sent.");
				return 0;
			}
			await _dbContext.Results.AddRangeAsync(result);
			return await _dbContext.SaveChangesAsync();
		}

		// GetResultById
		public async Task<Result> GetResultById(int id)
		{
			var result = await _dbContext.Results.FindAsync(id);
			if (result == null)
			{
				throw new KeyNotFoundException($"Error | Result with id {id} not found.");
			}
			return result;
		}

		// GetResultByAssetId
		public async Task<Result> GetResultByAssetId(int assetId)
		{
			var result = await _dbContext.Results.FirstOrDefaultAsync(r => r.AssetId == assetId);
			if (result == null)
			{
				throw new KeyNotFoundException($"Error | Result with Asset id {assetId} not found.");
			}
			return result;
		}

		// AddResult ( for a single result )
		public async Task<int> AddResult(int id, float heatProduction, float electricity , float productionCost, float primaryEnergyConsumed, int co2Produced, int assetId)
		{
			Result result = new Result
			{
				Id = id,
				HeatProduction = heatProduction,
				Electricity = electricity,
				ProductionCost = productionCost,
				PrimaryEnergyConsumed = primaryEnergyConsumed,
				CO2Produced = co2Produced,
				AssetId = assetId
			};
			await _dbContext.Results.AddAsync(result);
			return await _dbContext.SaveChangesAsync();
		}

		// UpdateResult
		public async Task<int> UpdateResult(int id, float heatProduction, float electricity, float productionCost, float primaryEnergyConsumed, int co2Produced, int assetId)
        {
			var result = await _dbContext.Results.FindAsync(id);
			if (result == null)
			{
				throw new KeyNotFoundException($"Error | Result with id {id} not found.");
			}

			result.HeatProduction = heatProduction;
			result.Electricity = electricity;
			result.ProductionCost = productionCost;
			result.PrimaryEnergyConsumed = primaryEnergyConsumed;
			result.CO2Produced = co2Produced;
			result.AssetId = assetId;

            return await _dbContext.SaveChangesAsync();
        }

		// DeleteResult
		public async Task<int> DeleteResult(int id)
		{
			var result = await _dbContext.Results.FindAsync(id);
			if (result == null)
			{
				throw new KeyNotFoundException($"Error | Result with id {id} not found.");
			}
			_dbContext.Results.Remove(result);
			return await _dbContext.SaveChangesAsync();
		}
	}
}
