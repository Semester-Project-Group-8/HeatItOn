using Microsoft.EntityFrameworkCore;
namespace Backend.Data
{
    public class BackendDbContext:DbContext
    {
        public BackendDbContext(DbContextOptions options) : base(options) { }
        public DbSet<Models.Demand> Demands { get; set; }


    }
}
