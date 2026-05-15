using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class OptimizedResultsService: IService<OptimizedResults>
{
    private readonly BackendDbContext _dbContext;
    

    public OptimizedResultsService(BackendDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    

    public async Task<List<OptimizedResults>> List()
    {
        return await _dbContext.OptimizedResults
            .Include(results => results.ResultsForHours)
            .ThenInclude(list => list.Results)
            .ThenInclude(r => r.Asset)
            .ToListAsync();
    }
    

    public async Task<OptimizedResults> Get(int id)
    {
        var result = await _dbContext.OptimizedResults.FirstOrDefaultAsync(o => o.Id == id);
        
        return result ?? throw new KeyNotFoundException($"OptimizedResults with ID {id} not found.");
    }

    public async Task Post(OptimizedResults optimizedResults)
    {
        var result = await _dbContext.OptimizedResults.AddAsync(optimizedResults);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Put(int id, OptimizedResults value)
    {
        var asset = await _dbContext.OptimizedResults.FirstOrDefaultAsync(o => o.Id == id);
        if (asset == null) throw new KeyNotFoundException($"OptimizedResults with ID {id} not found.");
        
        asset.Name = value.Name;
        
        _dbContext.OptimizedResults.Update(value);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var result = await _dbContext.OptimizedResults
            .Include(r => r.ResultsForHours)
                .ThenInclude(h => h.Results)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (result == null)
            throw new KeyNotFoundException($"OptimizedResults with ID {id} not found.");
        _dbContext.Results.RemoveRange(result.ResultsForHours.SelectMany(h => h.Results));
        _dbContext.ResultList.RemoveRange(result.ResultsForHours);
        _dbContext.OptimizedResults.Remove(result);
        await _dbContext.SaveChangesAsync();
    }
}
