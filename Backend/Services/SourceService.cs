using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
namespace Backend.Services
{
    public class SourceService:IService<Source>
    {
        private readonly BackendDbContext _dbContext;
        public SourceService(BackendDbContext dbContext)
        {
            _dbContext=dbContext;
        }

        public async Task<List<Source>> List()
        {
            try
            {
                return await _dbContext.Sources.ToListAsync();
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Sources could not be loaded.");
            }
        }
        public async Task<int> Post(List<Source> source)
        {
            if (source == null || source.Count == 0)
                throw new ArgumentException("No sources sent.");

            try
            {
                await _dbContext.Sources.AddRangeAsync(source);
                var result = await _dbContext.SaveChangesAsync();
                if (result <= 0)
                    throw new InvalidOperationException("Sources were not saved.");

                return result;
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException("Sources could not be saved due to a database error.");
            }
        }
        public Task<Source> Post() => throw new NotSupportedException("Use AddSource instead.");

        public async Task<Source> Get(int id)
        {
            var source = await _dbContext.Sources.FindAsync(id);
            if (source == null)
            {
                throw new KeyNotFoundException($"Source with ID {id} not found.");
            }

            return source;
        }

        public async Task Put(int id, Source value)
        {
            var source = await _dbContext.Sources.FindAsync(id);
            if (source == null)
                throw new KeyNotFoundException($"Source with ID {id} not found.");

            source.TimeFrom = value.TimeFrom;
            source.TimeTo = value.TimeTo;
            source.HeatDemand = value.HeatDemand;
            source.ElectricityPrice = value.ElectricityPrice;

            try
            {
                _dbContext.Sources.Update(source);
                var result = await _dbContext.SaveChangesAsync();
                if (result <= 0)
                    throw new InvalidOperationException("Source was not updated.");

            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException($"Source with ID {id} could not be updated due to a database error.");
            }
        }

        public async Task Delete(int id)
        {
            var source = await _dbContext.Sources.FindAsync(id);
            if (source == null)
                throw new KeyNotFoundException($"Source with ID {id} not found.");

            try
            {
                _dbContext.Sources.Remove(source);
                var result = await _dbContext.SaveChangesAsync();
                if (result <= 0)
                    throw new InvalidOperationException("Source was not deleted.");

            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException($"Source with ID {id} cannot be deleted due to existing dependencies.");
            }
        }

    }
}
