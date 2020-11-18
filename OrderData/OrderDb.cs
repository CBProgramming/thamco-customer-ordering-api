using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderData
{
    public class OrderDb : DbContext
    {

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<BasketItem> BasketItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderedItem> OrderedItems { get; set; }

        public OrderDb(DbContextOptions<OrderDb> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<BasketItem>()
                   .HasKey(b => new { b.CustomerId, b.ProductId });

            builder.Entity<OrderedItem>()
                    .HasKey(b => new { b.OrderId, b.ProductId });

            builder.Entity<Customer>()
                .Property(c => c.CustomerId)
                .ValueGeneratedNever();

            builder.Entity<Product>()
                .Property(c => c.ProductId)
                .ValueGeneratedNever();
        }
    }
}
