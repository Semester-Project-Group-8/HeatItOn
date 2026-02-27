using Microsoft.EntityFrameworkCore;
namespace Backend.Data
{
    public class BackendDbContext:DbContext
    {
        public BackendDbContext(DbContextOptions options) : base(options) { }
        public DbSet<Models.Demand> Demands { get; set; }
        public DbSet<Models.Asset> Assets{ get; set; }
        public DbSet<Models.Image> Images { get; set; }
        public DbSet<Models.Result> Results{ get; set; }


    }
}
