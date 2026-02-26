using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
namespace Backend.Services
{
    public class DemandService
    {
        private readonly BackendDbContext _dbContext;
        public DemandService(BackendDbContext dbContext)
        {
            _dbContext=dbContext;
        }
        public async Task<IEnumerable<Demand>> ListDemands() 
        {
            return await _dbContext.Demands.ToListAsync();
        }
        public async Task<int> AddDemands(List<Demand> demands)
        {
            if(demands==null || demands.Count==0)
            {
                Console.WriteLine("Error |  No demands sent.");
                return 0;
            }
            await _dbContext.Demands.AddRangeAsync(demands);
            return await _dbContext.SaveChangesAsync();
        }
        public async Task<int> AddDemand(int id, DateTime From, DateTime Til, float Heat, float Electro)
        {
            Demand demand = new Demand
            {
                ID= id,
                StartTime= From,
                EndTime= Til,
                HeatDemand=Heat,
                ElectricityPrice=Electro
            };
            await _dbContext.Demands.AddAsync(demand);
            return await _dbContext.SaveChangesAsync();
        }
        public async Task<IEnumerable<Demand>> ListByMonth(int month)
        {
            var demands = await _dbContext.Demands
                    .Where(d => d.StartTime.Month == month)
                    .ToListAsync();
            return demands;
        }
    }
}
