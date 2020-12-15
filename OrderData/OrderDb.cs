using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderData
{
    public class OrderDb : DbContext
    {

        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<BasketItem> BasketItems { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderedItem> OrderedItems { get; set; }

        public OrderDb(DbContextOptions<OrderDb> options) : base(options)
        {
        }

        public OrderDb()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema("ordering");

            builder.Entity<BasketItem>()
                   .HasKey(b => new { b.CustomerId, b.ProductId });

            builder.Entity<OrderedItem>()
                    .HasKey(b => new { b.OrderId, b.ProductId });

            builder.Entity<Customer>()
                .Property(c => c.CustomerId)
                // key is always provided
                .ValueGeneratedNever();

            builder.Entity<Product>()
                .Property(c => c.ProductId)
                .ValueGeneratedNever();

            builder.Entity<Customer>()
                .HasData(
                    new Customer
                    {
                        CustomerId = 1,
                        GivenName = "Chris",
                        FamilyName = "Burrell",
                        AddressOne = "85 Clifton Road",
                        Town = "Downtown",
                        State = "Durham",
                        AreaCode = "DL1 5RT",
                        EmailAddress = "t7145969@live.tees.ac.uk",
                        TelephoneNumber = "09876543210",
                        Active = true,
                        CanPurchase = true
                    },
                    new Customer
                    {
                        CustomerId = 2,
                        GivenName = "Fakie",
                        FamilyName = "McFakeFace",
                        AddressOne = "20 Fake Road",
                        Town = "FakeTown",
                        State = "FakeState",
                        AreaCode = "DLF AKE",
                        EmailAddress = "fake@live.tees.ac.uk",
                        TelephoneNumber = "01010101010",
                        Active = true,
                        CanPurchase = true
                    }
                );

            builder.Entity<Product>()
                .HasData(
                    new Product { ProductId = 1, Name = "Fake Product 1", Price = 1.99 },
                    new Product { ProductId = 2, Name = "Fake Product 2", Price = 2.98 },
                    new Product { ProductId = 3, Name = "Fake Product 3", Price = 3.97 }
                );

/*            builder.Entity<BasketItem>()
                .HasData(
                    new BasketItem { CustomerId = 1, ProductId = 1, Quantity = 5 },
                    new BasketItem { CustomerId = 1, ProductId = 2, Quantity = 3 }
                    );

            builder.Entity<Order>()
                .HasData(new Order { CustomerId = 1, OrderDate = new DateTime(2020, 1, 1), OrderId = 1, Total = 10.99 });*/

        }
    }
}
