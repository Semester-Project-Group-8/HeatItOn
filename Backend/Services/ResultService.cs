using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
namespace Backend.Services
{
	public class ResultService:IService<Result>
	{
		private readonly BackendDbContext _dbContext;

		public ResultService(BackendDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<List<Result>> List()
		{
			return await _dbContext.Results.ToListAsync();
		}

		public async Task<int> Post(List<Result> result)
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
		public async Task<Result> Get(int id)
		{
			var result = await _dbContext.Results.FindAsync(id);
			if (result == null)
				throw new KeyNotFoundException($"Error | Result with id {id} not found.");
			return  result;
		}

		// GetResultByAssetId
		public async Task<Result> GetResultByAssetId(int assetId)
		{
			var result = await _dbContext.Results.FirstOrDefaultAsync(r => r.AssetId == assetId);
			if (result == null)
				throw new KeyNotFoundException($"Error | Result with Asset id {assetId} not found.");
			return result;
		}

		public async Task Post(Result result)
		{
			await _dbContext.Results.AddAsync(result);
		}

		public async Task Put(int id, Result value)
		{
			var result = await _dbContext.Results.FindAsync(id);
			if (result == null)
				throw new KeyNotFoundException($"Error | Result with id {id} not found.");

			result.HeatProduction = value.HeatProduction;
			result.Electricity = value.Electricity;
			result.ProductionCost = value.ProductionCost;
			result.PrimaryEnergyConsumed = value.PrimaryEnergyConsumed;
			result.CO2Produced = value.CO2Produced;
			result.AssetId = value.AssetId;

			await _dbContext.SaveChangesAsync();
		}

		public async Task Delete(int id)
		{
			var result = await _dbContext.Results.FindAsync(id);
			if (result == null)
				throw new KeyNotFoundException($"Error | Result with id {id} not found.");
			_dbContext.Results.Remove(result);
			await _dbContext.SaveChangesAsync();
		}
	}
}
