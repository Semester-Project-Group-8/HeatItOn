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
    }
}
