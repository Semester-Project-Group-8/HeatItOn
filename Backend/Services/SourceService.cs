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
            try
            {
                return await _dbContext.Sources.ToListAsync();
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Sources could not be loaded.");
            }
        }
        public async Task<int> AddSources(List<Source> source)
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
        public async Task<int> AddSource(int id, DateTime From, DateTime Til, float Heat, float Electro)
        {
            var exists = await _dbContext.Sources.AnyAsync(s => s.Id == id);
            if (exists)
                throw new InvalidOperationException($"Source with ID {id} already exists.");

            Source source = new Source
            {
                Id= id,
                TimeFrom = From,
                TimeTo= Til,
                HeatDemand=Heat,
                ElectricityPrice=Electro
            };

            try
            {
                await _dbContext.Sources.AddAsync(source);
                var result = await _dbContext.SaveChangesAsync();
                if (result <= 0)
                    throw new InvalidOperationException("Source was not saved.");

                return result;
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException($"Source with ID {id} already exists.");
            }
        }
        public async Task<IEnumerable<Source>> ListByMonth(int month)
        {
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
        public async Task<Source> ListByHour(DateTime date)
        {
            return await _dbContext.Sources
                    .FirstOrDefaultAsync(d =>
                        d.TimeFrom.Date == date.Date &&
                        d.TimeFrom.Hour == date.Hour);
        }
        //update function - get item by id, update item, save changes
        public async Task<int> UpdateSource(int id, DateTime From, DateTime Til, float Heat, float Electro)
        {
            var source = await _dbContext.Sources.FindAsync(id);
            if (source == null)
            {
                throw new KeyNotFoundException($"Source with ID {id} not found.");
            }

            source.TimeFrom = From;
            source.TimeTo = Til;
            source.HeatDemand = Heat;
            source.ElectricityPrice = Electro;

            try
            {
                _dbContext.Sources.Update(source);
                var result = await _dbContext.SaveChangesAsync();
                if (result <= 0)
                    throw new InvalidOperationException("Source was not updated.");

                return result;
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException($"Source with ID {id} could not be updated due to a database error.");
            }
        }
        //delete - get item by id, delete item, save changes
        public async Task<int> DeleteSource(int id)
        {
            var source = await _dbContext.Sources.FindAsync(id);
            if (source == null)
            {
                throw new KeyNotFoundException($"Source with ID {id} not found.");
            }

            try
            {
                _dbContext.Sources.Remove(source);
                var result = await _dbContext.SaveChangesAsync();
                if (result <= 0)
                    throw new InvalidOperationException("Source was not deleted.");

                return result;
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException($"Source with ID {id} cannot be deleted due to existing dependencies.");
            }
        }

    }
}
