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
        return await _dbContext.OptimizedResults.Include(results => results.ResultsForHours).ThenInclude(list => list.Results).ThenInclude(r => r.Asset).ToListAsync();
    }
    

    public async Task<List<OptimizedResults>> Get(int id)
    {
        var result = await _dbContext.OptimizedResults.FirstOrDefaultAsync(o => o.Id == id);
        if (result == null)
            throw new KeyNotFoundException($"OptimizedResults with ID {id} not found.");
        return new List<OptimizedResults> { result };
    }

    public Task<OptimizedResults> Post() => throw new NotSupportedException("Use AddOptimizedResults instead.");

    public async Task<int> AddOptimizedResults(OptimizedResults optimizedResults)
    {
        await _dbContext.OptimizedResults.AddAsync(optimizedResults);
        return await _dbContext.SaveChangesAsync();
    }

    public async Task Put(int id, OptimizedResults value)
    {
        value.Id = id;
        _dbContext.OptimizedResults.Update(value);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var result = await _dbContext.OptimizedResults.FindAsync(id);
        if (result == null)
            throw new KeyNotFoundException($"OptimizedResults with ID {id} not found.");
        _dbContext.OptimizedResults.Remove(result);
        await _dbContext.SaveChangesAsync();
    }
}
