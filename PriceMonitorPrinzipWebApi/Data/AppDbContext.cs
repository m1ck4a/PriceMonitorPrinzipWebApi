using Microsoft.EntityFrameworkCore;
using PriceMonitorPrinzipWebApi.Models;

namespace PriceMonitorPrinzipWebApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AdUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CurrentPrice).IsRequired();
                entity.Property(e => e.LastChecked).IsRequired();
            });
        }
    }
}
