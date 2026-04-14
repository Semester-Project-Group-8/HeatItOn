using Microsoft.EntityFrameworkCore;
using Backend.Models;
namespace Backend.Data
{
    public class BackendDbContext:DbContext
    {
        public BackendDbContext(DbContextOptions options) : base(options) { }
        public DbSet<Source> Sources { get; set; }
        public DbSet<Asset> Assets{ get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Result> Results{ get; set; }
        public DbSet<ResultList> ResultList { get; set; }
        public DbSet<OptimizedResults> OptimizedResults { get; set; }
    }
}
