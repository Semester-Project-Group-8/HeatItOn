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
    }
}
