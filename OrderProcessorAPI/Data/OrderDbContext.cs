
using Microsoft.EntityFrameworkCore;
using OrderProcessorAPI.Models;

namespace OrderProcessorAPI.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .Property(o => o.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(i => i.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(i => i.Quantity)
                .HasPrecision(18, 3);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Beef Stew", Price = 105m },
                new Product { Id = 2, Name = "Beef Wors", Price = 105m },
                new Product { Id = 3, Name = "Lamb Chops", Price = 215m },
                new Product { Id = 4, Name = "Lamb Stew", Price = 150m },
                new Product { Id = 5, Name = "Lamb Shank", Price = 170m },
                new Product { Id = 6, Name = "Lamb Ribs", Price = 170m },
                new Product { Id = 7, Name = "Pork Ribs", Price = 100m }
            );
        }
    }
}
