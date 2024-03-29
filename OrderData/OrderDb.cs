﻿using Microsoft.EntityFrameworkCore;
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
                        CustomerAuthId = "a64c9beb-534a-4b40-a9be-58ed21597cd0",
                        GivenName = "Chris",
                        FamilyName = "Burrell",
                        AddressOne = "85 Clifton Road",
                        Town = "Downtown",
                        Country = "UK",
                        State = "Durham",
                        AreaCode = "DL1 5RT",
                        EmailAddress = "chris@example.com",
                        TelephoneNumber = "09876543210",
                        Active = true,
                        CanPurchase = true
                    },
                    new Customer
                    {
                        CustomerId = 2,
                        CustomerAuthId = "8e689e3c-24b1-400c-a8ad-7435c4fd15b5",
                        GivenName = "Paul",
                        FamilyName = "Mitchell",
                        AddressOne = "85 Clifton Road",
                        Town = "Downtown",
                        Country = "UK",
                        State = "Durham",
                        AreaCode = "DL1 5RT",
                        EmailAddress = "paul@example.com",
                        TelephoneNumber = "09876543210",
                        Active = true,
                        CanPurchase = true
                    },
                    new Customer
                    {
                        CustomerId = 3,
                        CustomerAuthId = "94d6c9b0-b3c8-4ad6-96ed-c7ab43d6dd23",
                        GivenName = "Jack",
                        FamilyName = "Ferguson",
                        AddressOne = "85 Clifton Road",
                        Town = "Downtown",
                        Country = "UK",
                        State = "Durham",
                        AreaCode = "DL1 5RT",
                        EmailAddress = "jack@example.com",
                        TelephoneNumber = "09876543210",
                        Active = true,
                        CanPurchase = true
                    },
                    new Customer
                    {
                        CustomerId = 4,
                        CustomerAuthId = "0313a3ca-e9d0-43c3-a580-ab25c6b224d8",
                        GivenName = "Carter",
                        FamilyName = "Ridgeway",
                        AddressOne = "85 Clifton Road",
                        Town = "Downtown",
                        Country = "UK",
                        State = "Durham",
                        AreaCode = "DL1 5RT",
                        EmailAddress = "carter@example.com",
                        TelephoneNumber = "09876543210",
                        Active = true,
                        CanPurchase = true
                    },
                    new Customer
                    {
                        CustomerId = 5,
                        CustomerAuthId = "8de93d90-7e62-40e9-8032-602f835ee8ee",
                        GivenName = "Karl",
                        FamilyName = "Hall",
                        AddressOne = "85 Clifton Road",
                        Town = "Downtown",
                        Country = "UK",
                        State = "Durham",
                        AreaCode = "DL1 5RT",
                        EmailAddress = "karl@example.com",
                        TelephoneNumber = "09876543210",
                        Active = true,
                        CanPurchase = true
                    }
                );

            builder.Entity<Product>()
                .HasData(
                    new Product { ProductId = 1, Name = "Fake Product 1", Price = 1.99, Quantity = 20 },
                    new Product { ProductId = 2, Name = "Fake Product 2", Price = 2.98, Quantity = 20 },
                    new Product { ProductId = 3, Name = "Fake Product 3", Price = 3.97, Quantity = 20 }
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
