using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
namespace Backend.Services
{
    public class ResultListService
    {
        private readonly BackendDbContext _dbContext;
        public ResultListService(BackendDbContext dbContext)
        {
            _dbContext=dbContext;
        }

        public async Task<IEnumerable<ResultList>> ListResultLists()
        {
            return await _dbContext.ResultList
                .Include(rl => rl.Results)
                .ToListAsync();
        }

        public async Task<ResultList> Get(int id)
        {
            var resultList = await _dbContext.ResultList
                .Include(rl => rl.Results)
                .FirstOrDefaultAsync(rl => rl.Id == id);

            if (resultList == null)
            {
                throw new KeyNotFoundException($"Result list with id {id} not found.");
            }

            return resultList;
        }

        public async Task<int> Put(DateTime timeFrom, DateTime timeTo, List<Result> resultList)
        {
            if (resultList == null || resultList.Count == 0)
            {
                throw new ArgumentException("No results sent.");
            }

            if (resultList.GroupBy(result => result.AssetId).Any(group => group.Count() > 1))
            {
                throw new ArgumentException("Only one result per asset is allowed in a single result list.");
            }

            var requestedAssetIds = resultList
                .Select(result => result.AssetId)
                .Distinct()
                .ToList();

            var existingAssetIds = await _dbContext.Assets
                .Where(asset => requestedAssetIds.Contains(asset.Id))
                .Select(asset => asset.Id)
                .ToListAsync();

            var missingAssetIds = requestedAssetIds
                .Except(existingAssetIds)
                .ToList();

            if (missingAssetIds.Count > 0)
            {
                throw new ArgumentException($"Unknown asset id(s): {string.Join(", ", missingAssetIds)}");
            }

            foreach (var result in resultList)
            {
                result.Id = 0;
            }

            var newResultList = new ResultList
            {
                TimeFrom = timeFrom,
                TimeTo = timeTo,
                Results = resultList
            };

            await _dbContext.ResultList.AddAsync(newResultList);
            await _dbContext.SaveChangesAsync();
            return newResultList.Id;
        }
        public async Task<int> Post(List<ResultList> results)
        {
            foreach (var result in results)
            {
                await _dbContext.ResultList.AddAsync(result);
            }
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteResultList(int id)
        {
            var resultList = await _dbContext.ResultList
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

            _dbContext.ResultList.Remove(resultList);
            return await _dbContext.SaveChangesAsync();
        }
    }
}