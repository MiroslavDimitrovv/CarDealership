using CarDealership.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarDealership.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Car> Cars => Set<Car>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Rental> Rentals => Set<Rental>();
        public DbSet<Sale> Sales => Set<Sale>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Car>()
                .Property(c => c.SalePrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Car>()
                .Property(c => c.RentPricePerDay)
                .HasPrecision(18, 2);

           modelBuilder.Entity<Rental>()
    .Property(r => r.TotalPrice)
    .HasPrecision(18, 2);

modelBuilder.Entity<Rental>()
    .Property(r => r.PricePerDay)
    .HasPrecision(18, 2);

            modelBuilder.Entity<Sale>()
                .Property(s => s.FinalPrice)
                .HasPrecision(18, 2);
        }
    }
}
