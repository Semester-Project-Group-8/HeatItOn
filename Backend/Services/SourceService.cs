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
            return await _dbContext.Sources.ToListAsync();
        }
        
        public async Task PostList(List<Source> source)
        {
            if (source == null || source.Count == 0)
                throw new ArgumentException("Source list cannot be empty");

            await _dbContext.Sources.AddRangeAsync(source);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Source>> ListByMonth(int month)
        {
            if (month < 1 || month > 12) throw new ArgumentException("Invalid month");
            return await _dbContext.Sources
                .Where(s => s.TimeFrom.Month == month)
                .ToListAsync();
        }

        public async Task Post(Source source)
        {
            if (await _dbContext.Sources.AnyAsync(s => s.Id == source.Id))
                throw new InvalidOperationException("Source with this ID already exists.");
            await _dbContext.Sources.AddAsync(source);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException("Source could not be saved.");
            }
        }
        
        public async Task<Source> Get(int id)
        {
            var source = await _dbContext.Sources.FindAsync(id);
            return source ??  throw new KeyNotFoundException($"Source with ID {id} not found.");
        }

        public async Task Put(int id, Source value)
        {
            var source = await _dbContext.Sources.FindAsync(id);
            if (source == null) throw new KeyNotFoundException($"Source with ID {id} not found.");

            source.TimeFrom = value.TimeFrom;
            source.TimeTo = value.TimeTo;
            source.HeatDemand = value.HeatDemand;
            source.ElectricityPrice = value.ElectricityPrice;

            _dbContext.Sources.Update(source);
            await _dbContext.SaveChangesAsync();
        }
        
        public async Task Delete(int id)
        {
            var source = await _dbContext.Sources.FindAsync(id);
            if (source == null)
                throw new KeyNotFoundException($"Source with ID {id} not found.");

            _dbContext.Sources.Remove(source);
            await _dbContext.SaveChangesAsync();
        }

    }
}
