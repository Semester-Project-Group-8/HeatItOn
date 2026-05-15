using Microsoft.EntityFrameworkCore;
using Backend.Models;
namespace Backend.Data
{
    public class BackendDbContext:DbContext
    {
        public BackendDbContext(DbContextOptions options) : base(options) { }
        
        public DbSet<Source> Sources { get; set; }
        public DbSet<Asset> Assets{ get; set; }
        public DbSet<Result> Results{ get; set; }
        public DbSet<ResultByHour> ResultList { get; set; }
        public DbSet<OptimizedResults> OptimizedResults { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Source>()
                .HasIndex(s => new {s.TimeFrom, s.TimeTo, s.HeatDemand, s.ElectricityPrice})
                .IsUnique();
        }
    }
    
}
