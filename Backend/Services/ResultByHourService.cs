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
    }
}