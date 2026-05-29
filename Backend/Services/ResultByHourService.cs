using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.ExceptionHandling.Exceptions;
using Backend.Models;
namespace Backend.Services
{
    public class ResultByHourService : IService<ResultByHour>
    {
        private readonly BackendDbContext _dbContext;
        public ResultByHourService(BackendDbContext dbContext)
        {
            _dbContext=dbContext;
        }

        public async Task<List<ResultByHour>> List()
        {
            return await _dbContext.ResultByHours
                .Include(rl => rl.Results)
                .ThenInclude(rl => rl.Asset)
                .ToListAsync();
        }

        public async Task<ResultByHour> Get(int id)
        {
            var resultList = await _dbContext.ResultByHours
                .Include(rl => rl.Results)
                .FirstOrDefaultAsync(rl => rl.Id == id);

            return resultList ?? throw new KeyNotFoundException($"Result list with id {id} not found.");
        }

        public async Task Put(int id, ResultByHour resultByHours)
        {
            var rBh = await _dbContext.ResultByHours.FirstOrDefaultAsync(rl => rl.Id == resultByHours.Id);
            if (rBh == null) throw new NotFoundException($"Result by id {id} not found.");
            
            rBh.TimeFrom = resultByHours.TimeFrom;
            rBh.TimeTo = resultByHours.TimeTo;
            rBh.Results = resultByHours.Results;

            _dbContext.Update(rBh);
            await _dbContext.SaveChangesAsync();
        }

        public async Task PostList(List<ResultByHour> results)
        {
            await _dbContext.ResultByHours.AddRangeAsync(results);
            await _dbContext.SaveChangesAsync();
        }
        
        public async Task Post(ResultByHour result)
        {
            await _dbContext.ResultByHours.AddAsync(result);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var resultList = await _dbContext.ResultByHours
                .Include(rl => rl.Results)
                .FirstOrDefaultAsync(rl => rl.Id == id);
            if (resultList == null)
            {
                throw new KeyNotFoundException($"Result list with id {id} not found.");
            }

            if (resultList.Results.Count > 0)
            {
                _dbContext.Results.RemoveRange(resultList.Results);
            }

            _dbContext.ResultByHours.Remove(resultList);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<int> Post(List<ResultByHour> results)
        {
            if (results == null || results.Count == 0) return 0;
            await PostList(results);
            return results.Count;
        }

        public async Task<List<ResultByHour>> ListResultLists()
        {
            return await List();
        }

        public async Task<int> Put(DateTime timeFrom, DateTime timeTo, List<Result> results)
        {
            if (results == null || results.Count == 0)
                throw new ArgumentException("Results cannot be empty");

            var duplicateAsset = results.GroupBy(r => r.AssetId).Any(g => g.Count() > 1);
            if (duplicateAsset)
                throw new ArgumentException("Same asset appears more than once in results");

            foreach (var r in results)
            {
                var exists = await _dbContext.Assets.FindAsync(r.AssetId);
                if (exists == null)
                    throw new ArgumentException($"Asset with id {r.AssetId} does not exist");
            }

            var rbh = new ResultByHour
            {
                TimeFrom = timeFrom,
                TimeTo = timeTo,
                Results = results
            };

            await _dbContext.ResultByHours.AddAsync(rbh);
            await _dbContext.SaveChangesAsync();

            return rbh.Id;
        }

        public async Task<int> DeleteResultList(int id)
        {
            await Delete(id);
            return 1;
        }
    }
}