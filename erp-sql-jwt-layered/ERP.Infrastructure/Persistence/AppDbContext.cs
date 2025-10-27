using ERP.Domain.Entities;
using ERP.Infrastructure.Identity;
using ERP.Infrastructure.Specifications;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        // Runtime için DI constructor
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<ReservationItem> ReservationItems => Set<ReservationItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Sku)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order!)
                .HasForeignKey(i => i.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId);

            // Reservation ? Customer (N:1)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Customer)
                .WithMany(c => c.Reservations)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reservation ? Order (N:1)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Order)
                .WithMany(o => o.Reservations)
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // ReservationItem ? Reservation (N:1)
            modelBuilder.Entity<ReservationItem>()
                .HasOne(ri => ri.Reservation)
                .WithMany(r => r.Items)
                .HasForeignKey(ri => ri.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            // ReservationItem ? Product (N:1)
            modelBuilder.Entity<ReservationItem>()
                .HasOne(ri => ri.Product)
                .WithMany()
                .HasForeignKey(ri => ri.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
