using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class OptimizedResultsService
{
    private readonly BackendDbContext _dbContext;
    
    public OptimizedResultsService(BackendDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<IEnumerable<OptimizedResults>> ListOptimizedResults()
    {
        return await _dbContext.OptimizedResults.Include(results => results.ResultsForHours).ThenInclude(list => list.Results).ThenInclude(r => r.Asset).ToListAsync();
    }
    
    public async Task<OptimizedResults?> GetOptimizedResults(int id)
    {
        return await _dbContext.OptimizedResults.FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<int> AddOptimizedResults(OptimizedResults optimizedResults)
    {
        await _dbContext.OptimizedResults.AddAsync(optimizedResults);
        var result = await _dbContext.SaveChangesAsync();
        return result;
    }

    public async Task<int> UpdateOptimizedResults(OptimizedResults optimizedResults)
    {
        _dbContext.OptimizedResults.Update(optimizedResults);
        var result = await _dbContext.SaveChangesAsync();
        return result;
    }

   	public async Task<int> DeleteOptimizedResult(int id)
		{
			var result = await _dbContext.OptimizedResults.FindAsync(id);
			_dbContext.OptimizedResults.Remove(result);
			return await _dbContext.SaveChangesAsync();
		}
}