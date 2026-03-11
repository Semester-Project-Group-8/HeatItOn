using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
namespace Backend.Services
{
    public class SourceService
    {
        private readonly BackendDbContext _dbContext;
        public SourceService(BackendDbContext dbContext)
        {
            _dbContext=dbContext;
        }
        public async Task<IEnumerable<Source>> ListSources() 
        {
            return await _dbContext.Sources.ToListAsync();
        }
        public async Task<int> AddSources(List<Source> source)
        {
            if(source == null || source.Count==0)
            {
                Console.WriteLine("Error |  No Sources sent.");
                return 0;
            }
            await _dbContext.Sources.AddRangeAsync(source);
            return await _dbContext.SaveChangesAsync();
        }
        public async Task<int> AddSource(int id, DateTime From, DateTime Til, float Heat, float Electro)
        {
            Source source = new Source
            {
                Id= id,
                TimeFrom = From,
                TimeTo= Til,
                HeatDemand=Heat,
                ElectricityPrice=Electro
            };
            await _dbContext.Sources.AddAsync(source);
            return await _dbContext.SaveChangesAsync();
        }
        public async Task<IEnumerable<Source>> ListByMonth(int month)
        {
            var demands = await _dbContext.Sources
                    .Where(d => d.TimeFrom.Month == month)
                    .ToListAsync();
            return demands;

        }
        //update function - get item by id, update item, save changes
        public async Task<int> UpdateSource(int id, DateTime From, DateTime Til, float Heat, float Electro)
        {
            var source = await _dbContext.Sources.FindAsync(id);
            if (source == null)
            {
                throw new KeyNotFoundException($"Source with id {id} not found.");
            }
            source.TimeFrom = From;
            source.TimeTo = Til;
            source.HeatDemand = Heat;
            source.ElectricityPrice = Electro;
            return await _dbContext.SaveChangesAsync();
        }
        //delete - get item by id, delete item, save changes
        public async Task<int> DeleteSource(int id)
        {
            var source = await _dbContext.Sources.FindAsync(id);
            if (source == null)
            {
                throw new KeyNotFoundException($"Source with id {id} not found.");
            }
            _dbContext.Sources.Remove(source);
            return await _dbContext.SaveChangesAsync();
        }

    }
}
