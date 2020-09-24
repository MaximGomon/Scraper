using Microsoft.EntityFrameworkCore;
using Scraper.Database.Entities;

namespace Scraper.Database
{
    public class ScraperDataContext : DbContext
    {
        public ScraperDataContext(DbContextOptions<ScraperDataContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Operation> Operations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dish>()
                .HasOne(x => x.Section)
                .WithMany(x => x.Dishes)
                .OnDelete(DeleteBehavior.Cascade)
                ;
        }
    }
}