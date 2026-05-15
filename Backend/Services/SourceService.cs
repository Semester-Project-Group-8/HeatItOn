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
        public async Task<Source> Post(Source source)
        {
            if (source == null)
                throw new ArgumentException("No sources sent.");
            await _dbContext.Sources.AddAsync(source);
            await _dbContext.SaveChangesAsync();
            return source;
        }

        public async Task<int> AddSource(int id, DateTime timeFrom, DateTime timeTo, float heatDemand, float electricityPrice)
        {
            if (heatDemand < 0)
                throw new ArgumentException("Heat demand cannot be negative.");

            if (timeFrom > timeTo)
                throw new ArgumentException("TimeFrom cannot be after TimeTo.");

            var source = new Source
            {
                Id = id,
                TimeFrom = timeFrom,
                TimeTo = timeTo,
                HeatDemand = heatDemand,
                ElectricityPrice = electricityPrice
            };

            await Post(source);
            return 1;
        }

        public async Task<int> AddSources(List<Source> sources)
        {
             await _dbContext.Sources.AddRangeAsync(sources);
             return await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Source>> ListByHour(DateTime date)
        {
             return await _dbContext.Sources.Where(s => s.TimeFrom.Date == date.Date && s.TimeFrom.Hour == date.Hour).ToListAsync();
        }
        
        public async Task<int> Post(List<Source> source)
        {
            if (source == null || source.Count == 0)
                throw new ArgumentException("No sources sent.");

            try
            {
                await _dbContext.Sources.AddRangeAsync(source);
                var result = await _dbContext.SaveChangesAsync();
                Console.WriteLine($"{result} sources uploaded");
                return result <= 0 ? throw new InvalidOperationException("Sources were not saved.") : result;
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException("Sources could not be saved due to a database error.");
            }
        }

        public async Task Post(Source source)
        {
            var result = await _dbContext.Sources.AddAsync(source);
        }

        public async Task<IEnumerable<Source>> ListByMonth(int month)
        {
            if (month < 1 || month > 12)
                throw new ArgumentException("Month must be between 1 and 12.");
            try
            {
                return await _dbContext.Sources
                    .Where(d => d.TimeFrom.Month == month)
                    .ToListAsync();
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Sources could not be loaded.");
            }

        }
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

        public Task Put(int id, DateTime timeFrom, DateTime timeTo, float heatDemand, float electricityPrice)
        {
            return Put(id, new Source
            {
                Id = id,
                TimeFrom = timeFrom,
                TimeTo = timeTo,
                HeatDemand = heatDemand,
                ElectricityPrice = electricityPrice
            });
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
