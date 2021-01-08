using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Order.Repository;
using Order.Repository.Models;
using OrderData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Order.Repository.Data;

namespace CustomerOrderingService.UnitTests
{
    public class RepositoryTests
    {
        public CustomerRepoModel customerRepoModel;
        public BasketItemRepoModel basketItemRepoModel;
        public ProductRepoModel productRepoModel;
        public List<OrderedItemRepoModel> orderedItemRepoModels;
        public List<ProductRepoModel> productRepoModels;
        public IMapper mapper;
        public IQueryable<Customer> dbCustomers;
        public IQueryable<Product> dbProducts;
        public IQueryable<BasketItem> dbBasketItems;
        public IQueryable<OrderData.Order> dbOrders;
        public IQueryable<OrderedItem> dbOrderedItems;
        public Customer dbCustomer1, dbCustomer2;
        public Product dbProduct1, dbProduct2, dbProduct3;
        public BasketItem dbBasketItem1, dbBasketItem2;
        public Mock<DbSet<Customer>> mockCustomers;
        public Mock<DbSet<Product>> mockProducts;
        public Mock<DbSet<BasketItem>> mockBasketItems;
        public Mock<DbSet<OrderedItem>> mockOrderedItems;
        public Mock<DbSet<OrderData.Order>> mockOrders;
        public Mock<OrderDb> mockDbContext;
        public OrderRepository repo;
        public CustomerRepoModel anonymisedCustomer;
        public OrderRepoModel orderRepoModel;
        public OrderData.Order order1, order2, order3;
        public OrderedItem orderedItem1, orderedItem2, orderedItem3, orderedItem4, orderedItem5;
        public FinalisedOrderRepoModel finalOrder;

        private void SetupCustomerRepoModel()
        {
            customerRepoModel = new CustomerRepoModel
            {
                CustomerId = 3,
                GivenName = "Fake3",
                FamilyName = "Name3",
                AddressOne = "Address2 3",
                AddressTwo = "Address2 3",
                Town = "Town3",
                State = "State3",
                AreaCode = "Area Code3",
                Country = "Country3",
                EmailAddress = "email@email.com3",
                TelephoneNumber = "07123456783",
                CanPurchase = true,
                Active = true
            };
        }

        private void SetupDbCustomer()
        {
            dbCustomer1 = new Customer
            {
                CustomerId = 1,
                GivenName = "Fake1",
                FamilyName = "Name1",
                AddressOne = "Address1 1",
                AddressTwo = "Address1 2",
                Town = "Town1",
                State = "State1",
                AreaCode = "Area Code1",
                Country = "Country1",
                EmailAddress = "email@email.com1",
                TelephoneNumber = "07123456781",
                CanPurchase = true,
                Active = true
            };
            dbCustomer2 = new Customer
            {
                CustomerId = 2,
                GivenName = "Fake2",
                FamilyName = "Name2",
                AddressOne = "Address1 2",
                AddressTwo = "Address2 2",
                Town = "Town2",
                State = "State2",
                AreaCode = "Area Code2",
                Country = "Country2",
                EmailAddress = "email@email.com2",
                TelephoneNumber = "07123456782",
                CanPurchase = true,
                Active = true
            };
        }

        private void SetupDbCustomers()
        {
            SetupDbCustomer();
            dbCustomers = new List<Customer>
            {
                dbCustomer1
            }.AsQueryable();
        }

        private void SetupMockCustomers()
        {
            mockCustomers = new Mock<DbSet<Customer>>();
            mockCustomers.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(dbCustomers.Provider);
            mockCustomers.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(dbCustomers.Expression);
            mockCustomers.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(dbCustomers.ElementType);
            mockCustomers.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(dbCustomers.GetEnumerator());
        }

        private void SetupProductRepoModel()
        {
            productRepoModel = new ProductRepoModel
            {
                ProductId = 1,
                Name = "Fake Product 4",
                Price = 3.99,
                Quantity = 5
            };
        }

        private void SetupProductRepoModels()
        {
            productRepoModels = new List<ProductRepoModel>
            {
                new ProductRepoModel {ProductId = 1, Name = "Name1", Price = 0.01, Quantity = 1},
                new ProductRepoModel {ProductId = 2, Name = "Name2", Price = 0.02, Quantity = 2}
            };
        }

        private void SetupIndividualDbProducts()
        {
            dbProduct1 = new Product
            {
                ProductId = 1,
                Name = "Fake Product 1",
                Price = 1.99,
                Quantity = 3
            };
            dbProduct2 = new Product
            {
                ProductId = 2,
                Name = "Fake Product 2",
                Price = 2.98,
                Quantity = 4
            };
            dbProduct3 = new Product
            {
                ProductId = 3,
                Name = "Fake Product 3",
                Price = 3.97,
                Quantity = 5
            };
        }

        private void SetupDbProducts()
        {
            SetupIndividualDbProducts();
            dbProducts = new List<Product>
            {
                dbProduct1,
                dbProduct2,
                dbProduct3
            }.AsQueryable();
        }

        private void SetupMockProducts()
        {
            mockProducts = new Mock<DbSet<Product>>();
            mockProducts.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(dbProducts.Provider);
            mockProducts.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(dbProducts.Expression);
            mockProducts.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(dbProducts.ElementType);
            mockProducts.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(dbProducts.GetEnumerator());
        }

        private void SetupBasketItemRepoModel()
        {
            basketItemRepoModel = new BasketItemRepoModel
            {
                CustomerId = 1,
                ProductId = 2,
                Quantity = 3
            };
        }

        private void SetupDbBasketItem()
        {
            dbBasketItem1 = new BasketItem
            {
                CustomerId = 1,
                ProductId = 1,
                Quantity = 5
            };
            dbBasketItem2 = new BasketItem
            {
                CustomerId = 2,
                ProductId = 3,
                Quantity = 10
            };
        }

        private void SetupDbBasketItems()
        {
            SetupDbBasketItem();
            dbBasketItems = new List<BasketItem>
            {
                dbBasketItem1,
                dbBasketItem2
            }.AsQueryable();
        }

        private void SetupMockBasketItems()
        {
            mockBasketItems = new Mock<DbSet<BasketItem>>();
            mockBasketItems.As<IQueryable<BasketItem>>().Setup(m => m.Provider).Returns(dbBasketItems.Provider).Verifiable();
            mockBasketItems.As<IQueryable<BasketItem>>().Setup(m => m.Expression).Returns(dbBasketItems.Expression).Verifiable();
            mockBasketItems.As<IQueryable<BasketItem>>().Setup(m => m.ElementType).Returns(dbBasketItems.ElementType).Verifiable();
            mockBasketItems.As<IQueryable<BasketItem>>().Setup(m => m.GetEnumerator()).Returns(dbBasketItems.GetEnumerator()).Verifiable();
        }

        private void SetupOrderRepoModel()
        {
            orderRepoModel = new OrderRepoModel
            {
                OrderId = 1,
                OrderDate = new DateTime(),
                Total = 9.99
            };
        }

        private void SetupDbOrder()
        {
            order1 = new OrderData.Order
            {
                CustomerId = 1,
                OrderId = 1,
                Total = 9.99
            };
            order2 = new OrderData.Order
            {
                CustomerId = 1,
                OrderId = 2,
                Total = 19.99
            };
            order3 = new OrderData.Order
            {
                CustomerId = 2,
                OrderId = 3,
                Total = 19.99
            };
        }

        private void SetupDbOrders()
        {
            SetupDbOrder();
            dbOrders = new List<OrderData.Order>
            {
                order1,
                order2
            }.AsQueryable();
        }

        private void SetupMockOrders()
        {
            mockOrders = new Mock<DbSet<OrderData.Order>>();
            mockOrders.As<IQueryable<OrderData.Order>>().Setup(m => m.Provider).Returns(dbOrders.Provider).Verifiable();
            mockOrders.As<IQueryable<OrderData.Order>>().Setup(m => m.Expression).Returns(dbOrders.Expression).Verifiable();
            mockOrders.As<IQueryable<OrderData.Order>>().Setup(m => m.ElementType).Returns(dbOrders.ElementType).Verifiable();
            mockOrders.As<IQueryable<OrderData.Order>>().Setup(m => m.GetEnumerator()).Returns(dbOrders.GetEnumerator()).Verifiable();
        }

        private void SetupOrderedItemRepoModels()
        {
            orderedItemRepoModels = new List<OrderedItemRepoModel>
            {
                new OrderedItemRepoModel
                {
                    OrderId = 1,
                    ProductId = 1,
                    Quantity = 2,
                    Price = 5.99,
                    Name = "Item Name"
                },
                new OrderedItemRepoModel
                {
                    OrderId = 1,
                    ProductId = 2,
                    Quantity = 3,
                    Price = 6.99,
                    Name = "Item Name2"
                },
            };
        }

        private void SetupFinalisedOrderRepoModel()
        {
            finalOrder = new FinalisedOrderRepoModel
            {
                OrderId = 1,
                CustomerId = 1,
                OrderDate = new DateTime(),
                OrderedItems = orderedItemRepoModels,
                Total = 99.99
            };
        }

        private void SetupDbOrderedItem()
        {
            orderedItem1 = new OrderedItem
            {
                OrderId = 1,
                ProductId = 1,
                Quantity = 1,
                Price = 1,
                Name = "Name 1"
            };
            orderedItem2 = new OrderedItem
            {
                OrderId = 1,
                ProductId = 2,
                Quantity = 2,
                Price = 2,
                Name = "Name "
            };
            orderedItem3 = new OrderedItem
            {
                OrderId = 1,
                ProductId = 3,
                Quantity = 3,
                Price = 3,
                Name = "Name 3"
            };
            orderedItem4 = new OrderedItem
            {
                OrderId = 2,
                ProductId = 4,
                Quantity = 4,
                Price = 4,
                Name = "Name 4"
            };
            orderedItem5 = new OrderedItem
            {
                OrderId = 2,
                ProductId = 5,
                Quantity = 5,
                Price = 5,
                Name = "Name 5"
            };
        }

        private void SetupDbOrderedItems()
        {
            SetupDbOrderedItem();
            dbOrderedItems = new List<OrderedItem>
            {
                orderedItem1,
                orderedItem2,
                orderedItem3,
                orderedItem4,
                orderedItem5
            }.AsQueryable();
        }

        private void SetupMockOrderedItems()
        {
            mockOrderedItems = new Mock<DbSet<OrderedItem>>();
            mockOrderedItems.As<IQueryable<OrderedItem>>().Setup(m => m.Provider).Returns(dbOrderedItems.Provider).Verifiable();
            mockOrderedItems.As<IQueryable<OrderedItem>>().Setup(m => m.Expression).Returns(dbOrderedItems.Expression).Verifiable();
            mockOrderedItems.As<IQueryable<OrderedItem>>().Setup(m => m.ElementType).Returns(dbOrderedItems.ElementType).Verifiable();
            mockOrderedItems.As<IQueryable<OrderedItem>>().Setup(m => m.GetEnumerator()).Returns(dbOrderedItems.GetEnumerator()).Verifiable();
        }

        private void SetupMockDbContext()
        {
            mockDbContext = new Mock<OrderDb>();
            mockDbContext.Setup(m => m.Customers).Returns(mockCustomers.Object);
            mockDbContext.Setup(m => m.Products).Returns(mockProducts.Object);
            mockDbContext.Setup(m => m.BasketItems).Returns(mockBasketItems.Object);
            mockDbContext.Setup(m => m.Orders).Returns(mockOrders.Object);
            mockDbContext.Setup(m => m.OrderedItems).Returns(mockOrderedItems.Object);
        }


        private void SetupAnonCustomer()
        {
            anonymisedCustomer = new CustomerRepoModel
            {
                CustomerId = 1,
                GivenName = "Anonymised",
                FamilyName = "Anonymised",
                AddressOne = "Anonymised",
                AddressTwo = "Anonymised",
                Town = "Anonymised",
                State = "Anonymised",
                AreaCode = "Anonymised",
                Country = "Anonymised",
                EmailAddress = "anon@anon.com",
                TelephoneNumber = "00000000000",
                CanPurchase = false,
                Active = false
            };
        }

        private void SetupMapper()
        {
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            }).CreateMapper();
        }

        private void DefaultSetup()
        {
            SetupMapper();
            SetupCustomerRepoModel();
            SetupDbCustomers();
            SetupMockCustomers();
            SetupProductRepoModel();
            SetupProductRepoModels();
            SetupDbProducts();
            SetupMockProducts();
            SetupBasketItemRepoModel();
            SetupDbBasketItems();
            SetupMockBasketItems();
            SetupOrderRepoModel();
            SetupDbOrders();
            SetupMockOrders();
            SetupOrderedItemRepoModels();
            SetupFinalisedOrderRepoModel();
            SetupDbOrderedItems();
            SetupMockOrderedItems();
            SetupMockDbContext();
            repo = new OrderRepository(mockDbContext.Object, mapper);
        }

        [Fact]
        public async Task NewCustomer_ShouldTrue()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.NewCustomer(customerRepoModel);

            //Assert
            Assert.True(customerRepoModel.CustomerId == result);

            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Once());
        }

        [Fact]
        public async Task NewNullCustomer_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.NewCustomer(null);

            //Assert
            Assert.True(0 == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task EditCustomer_ShouldTrue()
        {
            //Arrange
            DefaultSetup();
            customerRepoModel.CustomerId = dbCustomer1.CustomerId;

            //Act
            var result = await repo.EditCustomer(customerRepoModel);

            //Assert
            Assert.True(true == result);
            Assert.Equal(dbCustomer1.CustomerId, customerRepoModel.CustomerId);
            Assert.Equal(dbCustomer1.GivenName, customerRepoModel.GivenName);
            Assert.Equal(dbCustomer1.FamilyName, customerRepoModel.FamilyName);
            Assert.Equal(dbCustomer1.AddressOne, customerRepoModel.AddressOne);
            Assert.Equal(dbCustomer1.AddressTwo, customerRepoModel.AddressTwo);
            Assert.Equal(dbCustomer1.Town, customerRepoModel.Town);
            Assert.Equal(dbCustomer1.State, customerRepoModel.State);
            Assert.Equal(dbCustomer1.AreaCode, customerRepoModel.AreaCode);
            Assert.Equal(dbCustomer1.Country, customerRepoModel.Country);
            Assert.Equal(dbCustomer1.EmailAddress, customerRepoModel.EmailAddress);
            Assert.Equal(dbCustomer1.TelephoneNumber, customerRepoModel.TelephoneNumber);
            Assert.Equal(dbCustomer1.CanPurchase, customerRepoModel.CanPurchase);
            Assert.Equal(dbCustomer1.Active, customerRepoModel.Active);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task EditCustomer_DoesntExist_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.EditCustomer(customerRepoModel);

            //Assert
            Assert.True(false == result);
            Assert.NotEqual(dbCustomer1.CustomerId, customerRepoModel.CustomerId);
            Assert.NotEqual(dbCustomer1.GivenName, customerRepoModel.GivenName);
            Assert.NotEqual(dbCustomer1.FamilyName, customerRepoModel.FamilyName);
            Assert.NotEqual(dbCustomer1.AddressOne, customerRepoModel.AddressOne);
            Assert.NotEqual(dbCustomer1.AddressTwo, customerRepoModel.AddressTwo);
            Assert.NotEqual(dbCustomer1.Town, customerRepoModel.Town);
            Assert.NotEqual(dbCustomer1.State, customerRepoModel.State);
            Assert.NotEqual(dbCustomer1.AreaCode, customerRepoModel.AreaCode);
            Assert.NotEqual(dbCustomer1.Country, customerRepoModel.Country);
            Assert.NotEqual(dbCustomer1.EmailAddress, customerRepoModel.EmailAddress);
            Assert.NotEqual(dbCustomer1.TelephoneNumber, customerRepoModel.TelephoneNumber);
            Assert.Equal(dbCustomer1.CanPurchase, customerRepoModel.CanPurchase);
            Assert.Equal(dbCustomer1.Active, customerRepoModel.Active);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task EditCustomer_Null_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.EditCustomer(null);

            //Assert
            Assert.True(false == result);
            Assert.NotEqual(dbCustomer1.CustomerId, customerRepoModel.CustomerId);
            Assert.NotEqual(dbCustomer1.GivenName, customerRepoModel.GivenName);
            Assert.NotEqual(dbCustomer1.FamilyName, customerRepoModel.FamilyName);
            Assert.NotEqual(dbCustomer1.AddressOne, customerRepoModel.AddressOne);
            Assert.NotEqual(dbCustomer1.AddressTwo, customerRepoModel.AddressTwo);
            Assert.NotEqual(dbCustomer1.Town, customerRepoModel.Town);
            Assert.NotEqual(dbCustomer1.State, customerRepoModel.State);
            Assert.NotEqual(dbCustomer1.AreaCode, customerRepoModel.AreaCode);
            Assert.NotEqual(dbCustomer1.Country, customerRepoModel.Country);
            Assert.NotEqual(dbCustomer1.EmailAddress, customerRepoModel.EmailAddress);
            Assert.NotEqual(dbCustomer1.TelephoneNumber, customerRepoModel.TelephoneNumber);
            Assert.Equal(dbCustomer1.CanPurchase, customerRepoModel.CanPurchase);
            Assert.Equal(dbCustomer1.Active, customerRepoModel.Active);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetExistingCustomer_ShouldOk()
        {
            //Arrange
            DefaultSetup();

            //Act
            var customer = await repo.GetCustomer(1);

            //Assert
            Assert.NotNull(customer);
            Assert.Equal(dbCustomer1.CustomerId, customer.CustomerId);
            Assert.Equal(dbCustomer1.GivenName, customer.GivenName);
            Assert.Equal(dbCustomer1.FamilyName, customer.FamilyName);
            Assert.Equal(dbCustomer1.AddressOne, customer.AddressOne);
            Assert.Equal(dbCustomer1.AddressTwo, customer.AddressTwo);
            Assert.Equal(dbCustomer1.Town, customer.Town);
            Assert.Equal(dbCustomer1.State, customer.State);
            Assert.Equal(dbCustomer1.AreaCode, customer.AreaCode);
            Assert.Equal(dbCustomer1.Country, customer.Country);
            Assert.Equal(dbCustomer1.EmailAddress, customer.EmailAddress);
            Assert.Equal(dbCustomer1.TelephoneNumber, customer.TelephoneNumber);
            Assert.Equal(dbCustomer1.CanPurchase, customer.CanPurchase);
            Assert.Equal(dbCustomer1.Active, customer.Active);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetCustomer_DoesntExists_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            var customer = await repo.GetCustomer(2);

            //Assert
            Assert.Null(customer);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task AnonymiseCustomer_ShouldTrue()
        {
            //Arrange
            DefaultSetup();
            SetupAnonCustomer();

            //Act
            var result = await repo.AnonymiseCustomer(anonymisedCustomer);

            //Assert
            Assert.True(true == result);
            Assert.Equal(dbCustomer1.CustomerId, anonymisedCustomer.CustomerId);
            Assert.Equal(dbCustomer1.GivenName, anonymisedCustomer.GivenName);
            Assert.Equal(dbCustomer1.FamilyName, anonymisedCustomer.FamilyName);
            Assert.Equal(dbCustomer1.AddressOne, anonymisedCustomer.AddressOne);
            Assert.Equal(dbCustomer1.AddressTwo, anonymisedCustomer.AddressTwo);
            Assert.Equal(dbCustomer1.Town, anonymisedCustomer.Town);
            Assert.Equal(dbCustomer1.State, anonymisedCustomer.State);
            Assert.Equal(dbCustomer1.AreaCode, anonymisedCustomer.AreaCode);
            Assert.Equal(dbCustomer1.Country, anonymisedCustomer.Country);
            Assert.Equal(dbCustomer1.EmailAddress, anonymisedCustomer.EmailAddress);
            Assert.Equal(dbCustomer1.TelephoneNumber, anonymisedCustomer.TelephoneNumber);
            Assert.Equal(dbCustomer1.CanPurchase, anonymisedCustomer.CanPurchase);
            Assert.Equal(dbCustomer1.Active, anonymisedCustomer.Active);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task AnonymiseCustomer_DoesntExist_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            SetupAnonCustomer();
            anonymisedCustomer.CustomerId = 3;

            //Act
            var result = await repo.AnonymiseCustomer(anonymisedCustomer);

            //Assert
            Assert.True(false == result);
            Assert.NotEqual(dbCustomer1.CustomerId, anonymisedCustomer.CustomerId);
            Assert.NotEqual(dbCustomer1.GivenName, anonymisedCustomer.GivenName);
            Assert.NotEqual(dbCustomer1.FamilyName, anonymisedCustomer.FamilyName);
            Assert.NotEqual(dbCustomer1.AddressOne, anonymisedCustomer.AddressOne);
            Assert.NotEqual(dbCustomer1.AddressTwo, anonymisedCustomer.AddressTwo);
            Assert.NotEqual(dbCustomer1.Town, anonymisedCustomer.Town);
            Assert.NotEqual(dbCustomer1.State, anonymisedCustomer.State);
            Assert.NotEqual(dbCustomer1.AreaCode, anonymisedCustomer.AreaCode);
            Assert.NotEqual(dbCustomer1.Country, anonymisedCustomer.Country);
            Assert.NotEqual(dbCustomer1.EmailAddress, anonymisedCustomer.EmailAddress);
            Assert.NotEqual(dbCustomer1.TelephoneNumber, anonymisedCustomer.TelephoneNumber);
            Assert.NotEqual(dbCustomer1.CanPurchase, anonymisedCustomer.CanPurchase);
            Assert.NotEqual(dbCustomer1.Active, anonymisedCustomer.Active);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task AnonymiseCustomer_Null_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            SetupAnonCustomer();
            anonymisedCustomer.CustomerId = 3;

            //Act
            var result = await repo.AnonymiseCustomer(null);

            //Assert
            Assert.True(false == result);
            Assert.NotEqual(dbCustomer1.CustomerId, anonymisedCustomer.CustomerId);
            Assert.NotEqual(dbCustomer1.GivenName, anonymisedCustomer.GivenName);
            Assert.NotEqual(dbCustomer1.FamilyName, anonymisedCustomer.FamilyName);
            Assert.NotEqual(dbCustomer1.AddressOne, anonymisedCustomer.AddressOne);
            Assert.NotEqual(dbCustomer1.AddressTwo, anonymisedCustomer.AddressTwo);
            Assert.NotEqual(dbCustomer1.Town, anonymisedCustomer.Town);
            Assert.NotEqual(dbCustomer1.State, anonymisedCustomer.State);
            Assert.NotEqual(dbCustomer1.AreaCode, anonymisedCustomer.AreaCode);
            Assert.NotEqual(dbCustomer1.Country, anonymisedCustomer.Country);
            Assert.NotEqual(dbCustomer1.EmailAddress, anonymisedCustomer.EmailAddress);
            Assert.NotEqual(dbCustomer1.TelephoneNumber, anonymisedCustomer.TelephoneNumber);
            Assert.NotEqual(dbCustomer1.CanPurchase, anonymisedCustomer.CanPurchase);
            Assert.NotEqual(dbCustomer1.Active, anonymisedCustomer.Active);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task CustomerExists_ShouldTrue()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.CustomerExists(dbCustomer1.CustomerId);

            //Assert
            Assert.True(true == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task CustomerExists_DoesntExist_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.CustomerExists(99);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task IsCustomerActive__ShouldTrue()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.IsCustomerActive(dbCustomer1.CustomerId);

            //Assert
            Assert.True(true == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task IsCustomerActive_DoesntExist__ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.IsCustomerActive(99);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task IsCustomerActive_NotActive__ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            dbCustomer1.Active = false;

            //Act
            var result = await repo.IsCustomerActive(dbCustomer1.CustomerId);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetBasketItem_ShouldReturnBasket()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;
            int expectedSize = dbBasketItems.Where(b => b.CustomerId == customerId).Count();

            //Act
            var basket = await repo.GetBasket(customerId);

            //Assert
            Assert.NotNull(basket);
            Assert.Equal(expectedSize, basket.Count);
            Assert.Equal(dbBasketItem1.CustomerId, basket[0].CustomerId);
            Assert.Equal(dbBasketItem1.ProductId, basket[0].ProductId);
            Assert.Equal(dbBasketItem1.Quantity, basket[0].Quantity);
            Assert.Equal(dbProduct1.Name, basket[0].ProductName);
            Assert.Equal(dbProduct1.Price, basket[0].Price);
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetBasketItem_CustomerDoesntExist_BasketShouldBeEmpty()
        {
            //Arrange
            DefaultSetup();
            int customerId = 3;

            //Act
            var basket = await repo.GetBasket(customerId);

            //Assert
            Assert.NotNull(basket);
            Assert.Equal(0, basket.Count);
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task AddBasketItem_NewItem_ShouldTrue()
        {
            //Arrange
            DefaultSetup();

            //Act
            bool result = await repo.AddBasketItem(basketItemRepoModel);

            //Assert
            Assert.True(true == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Once());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task AddBasketItem_NullItem_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            bool result = await repo.AddBasketItem(null);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task AddBasketItem_ItemAlreadyInBasket_ShouldTrueEditingItemQuantity()
        {
            //Arrange
            DefaultSetup();
            basketItemRepoModel.ProductId = 1;
            basketItemRepoModel.Quantity = 99;

            //Act
            bool result = await repo.AddBasketItem(basketItemRepoModel);

            //Assert
            Assert.True(true == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.Equal(basketItemRepoModel.ProductId, dbBasketItem1.ProductId);
            Assert.Equal(basketItemRepoModel.CustomerId, dbBasketItem1.CustomerId);
            Assert.Equal(basketItemRepoModel.Quantity, dbBasketItem1.Quantity);
        }

        [Fact]
        public async Task AddBasketItem_ProductDoesntExist_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            basketItemRepoModel.ProductId = 99;

            //Act
            bool result = await repo.AddBasketItem(basketItemRepoModel);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task AddBasketItem_CustomerDoesntExist_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            basketItemRepoModel.CustomerId = 99;

            //Act
            bool result = await repo.AddBasketItem(basketItemRepoModel);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task EditBasketItem_NewItem_ShouldTrueAddingNewItemToBasket()
        {
            //Arrange
            DefaultSetup();

            //Act
            bool result = await repo.EditBasketItem(basketItemRepoModel);

            //Assert
            Assert.True(true == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Once());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task EditBasketItem_NullItem_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            bool result = await repo.EditBasketItem(null);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task EditBasketItem_ItemAlreadyInBasket_ShouldTrue()
        {
            //Arrange
            DefaultSetup();
            basketItemRepoModel.ProductId = 1;
            basketItemRepoModel.Quantity = 99;

            //Act
            bool result = await repo.EditBasketItem(basketItemRepoModel);

            //Assert
            Assert.True(true == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.Equal(basketItemRepoModel.ProductId, dbBasketItem1.ProductId);
            Assert.Equal(basketItemRepoModel.CustomerId, dbBasketItem1.CustomerId);
            Assert.Equal(basketItemRepoModel.Quantity, dbBasketItem1.Quantity);
        }

        [Fact]
        public async Task EditBasketItem_ProductDoesntExist_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            basketItemRepoModel.ProductId = 99;

            //Act
            bool result = await repo.EditBasketItem(basketItemRepoModel);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task EditBasketItem_CustomerDoesntExist_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            basketItemRepoModel.CustomerId = 99;

            //Act
            bool result = await repo.EditBasketItem(basketItemRepoModel);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task IsItemInBasket_ShouldTrue()
        {
            //Arrange
            DefaultSetup();

            //Act
            bool result = await repo.IsItemInBasket(dbCustomer1.CustomerId,dbProduct1.ProductId);

            //Assert
            Assert.True(true == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task IsItemInBasket_WrongCustomer_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            bool result = await repo.IsItemInBasket(dbCustomer2.CustomerId, dbProduct1.ProductId);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task IsItemInBasket_WrongProduct_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            bool result = await repo.IsItemInBasket(dbCustomer1.CustomerId, dbProduct2.ProductId);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task IsItemInBasket_CustomerDoesntExist_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            bool result = await repo.IsItemInBasket(99, dbProduct1.ProductId);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task IsItemInBasket_ProductDoesntExist_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            bool result = await repo.IsItemInBasket(dbCustomer1.CustomerId, 99);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task IsItemInBasket_CustomerAndProductDontExist_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            bool result = await repo.IsItemInBasket(99, 99);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task DeleteBasketItem_ShouldTrue()
        {
            //Arrange
            DefaultSetup();

            //Act
            bool result = await repo.DeleteBasketItem(dbCustomer1.CustomerId, dbProduct1.ProductId);

            //Assert
            Assert.True(true == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Once());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task DeleteBasketItem_WrongCustomer_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            bool result = await repo.DeleteBasketItem(dbCustomer2.CustomerId, dbProduct1.ProductId);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task DeleteBasketItem_WrongProduct_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            bool result = await repo.DeleteBasketItem(dbCustomer1.CustomerId, dbProduct2.ProductId);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task DeleteBasketItem_CustomerDoesntExist_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            bool result = await repo.DeleteBasketItem(99, dbProduct1.ProductId);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task DeleteBasketItem_ProductDoesntExist_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            bool result = await repo.DeleteBasketItem(dbCustomer1.CustomerId, 99);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task DeleteBasketItem_CustomerAndProductDontExist_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            bool result = await repo.DeleteBasketItem(99, 99);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task CreateProduct_ShouldTrue()
        {
            //Arrange
            DefaultSetup();
            productRepoModel.ProductId = 4;

            //Act
            var result = await repo.CreateProduct(productRepoModel);

            //Assert
            Assert.True(true == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Once());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task CreateProduct_AlreadyExists_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.CreateProduct(productRepoModel);

            //Assert
            Assert.True(true == result);
            Assert.Equal(productRepoModel.ProductId, dbProduct1.ProductId);
            Assert.NotEqual(productRepoModel.Name, dbProduct1.Name);
            Assert.NotEqual(productRepoModel.Quantity, dbProduct1.Quantity);
            Assert.NotEqual(productRepoModel.Price, dbProduct1.Price);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Once());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task CreateNullProduct_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.CreateProduct(null);

            //Assert
            Assert.True(false == result);
            Assert.Equal(productRepoModel.ProductId, dbProduct1.ProductId);
            Assert.NotEqual(productRepoModel.Name, dbProduct1.Name);
            Assert.NotEqual(productRepoModel.Quantity, dbProduct1.Quantity);
            Assert.NotEqual(productRepoModel.Price, dbProduct1.Price);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task EditProduct_ShouldTrue()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.EditProduct(productRepoModel);

            //Assert
            Assert.True(true == result);
            Assert.Equal(productRepoModel.ProductId, dbProduct1.ProductId);
            Assert.Equal(productRepoModel.Name, dbProduct1.Name);
            Assert.Equal(productRepoModel.Quantity, dbProduct1.Quantity);
            Assert.Equal(productRepoModel.Price, dbProduct1.Price);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task EditProduct_DoesntExist_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            productRepoModel.ProductId = 4;

            //Act
            var result = await repo.EditProduct(productRepoModel);

            //Assert
            Assert.True(false == result);
            Assert.NotEqual(productRepoModel.ProductId, dbProduct1.ProductId);
            Assert.NotEqual(productRepoModel.Name, dbProduct1.Name);
            Assert.NotEqual(productRepoModel.Quantity, dbProduct1.Quantity);
            Assert.NotEqual(productRepoModel.Price, dbProduct1.Price);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task EditProduct_Null_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.EditProduct(null);

            //Assert
            Assert.True(false == result);
            Assert.Equal(productRepoModel.ProductId, dbProduct1.ProductId);
            Assert.NotEqual(productRepoModel.Name, dbProduct1.Name);
            Assert.NotEqual(productRepoModel.Quantity, dbProduct1.Quantity);
            Assert.NotEqual(productRepoModel.Price, dbProduct1.Price);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task DeleteProduct_ShouldTrue()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.DeleteProduct(1);

            //Assert
            Assert.True(true == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Once());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task DeleteProduct_DoesntExist_ShouldTrue()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.DeleteProduct(4);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ProductExists_ShouldTrue()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.ProductExists(1);

            //Assert
            Assert.True(true == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ProductExists_DoesntExist_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.ProductExists(4);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetExistingOrder_ShouldReturnOrder()
        {
            //Arrange
            DefaultSetup();

            //Act
            var order = await repo.GetCustomerOrder(1);

            //Assert
            Assert.NotNull(order);
            Assert.Equal(order1.OrderId, order.OrderId);
            Assert.Equal(order1.OrderDate, order.OrderDate);
            Assert.Equal(order1.Total, order.Total);
            Assert.Equal(order1.OrderedItems.Count, order.OrderedItems.Count);
            for (int i = 0; i < order.OrderedItems.Count; i++)
            {
                Assert.Equal(order1.OrderedItems[i].OrderId, order.OrderedItems[i].OrderId);
                Assert.Equal(order1.OrderedItems[i].ProductId, order.OrderedItems[i].ProductId);
                Assert.Equal(order1.OrderedItems[i].Quantity, order.OrderedItems[i].Quantity);
                Assert.Equal(order1.OrderedItems[i].Price, order.OrderedItems[i].Price);
                Assert.Equal(order1.OrderedItems[i].Name, order.OrderedItems[i].Name);
            }

            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderData.Order>()), Times.Never());
            mockOrders.Verify(m => m.Remove(It.IsAny<OrderData.Order>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderedItem>()), Times.Never());
            mockOrderedItems.Verify(m => m.Remove(It.IsAny<OrderedItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetOrderDoesntExist_ShouldNull()
        {
            //Arrange
            DefaultSetup();

            //Act
            var order = await repo.GetCustomerOrder(99);

            //Assert
            Assert.Null(order);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderData.Order>()), Times.Never());
            mockOrders.Verify(m => m.Remove(It.IsAny<OrderData.Order>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderedItem>()), Times.Never());
            mockOrderedItems.Verify(m => m.Remove(It.IsAny<OrderedItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetOrderNoItems_ShouldNull()
        {
            //Arrange
            DefaultSetup();
            orderedItem1.OrderId = 2;
            orderedItem2.OrderId = 2;
            orderedItem3.OrderId = 2;

            //Act
            var order = await repo.GetCustomerOrder(1);

            //Assert
            Assert.Null(order);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderData.Order>()), Times.Never());
            mockOrders.Verify(m => m.Remove(It.IsAny<OrderData.Order>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderedItem>()), Times.Never());
            mockOrderedItems.Verify(m => m.Remove(It.IsAny<OrderedItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetCustomerOrders_ShouldReturnOrders()
        {
            //Arrange
            DefaultSetup();

            //Act
            var orders = await repo.GetCustomerOrders(1);

            //Assert
            Assert.NotNull(orders);
            Assert.Equal(dbOrders.Count(), orders.Count);
            Assert.Equal(order1.OrderDate, orders[0].OrderDate);
            Assert.Equal(order1.Total, orders[0].Total);
            Assert.Equal(order1.OrderId, orders[0].OrderId);
            Assert.Equal(order2.OrderDate, orders[1].OrderDate);
            Assert.Equal(order2.Total, orders[1].Total);
            Assert.Equal(order2.OrderId, orders[1].OrderId);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderData.Order>()), Times.Never());
            mockOrders.Verify(m => m.Remove(It.IsAny<OrderData.Order>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderedItem>()), Times.Never());
            mockOrderedItems.Verify(m => m.Remove(It.IsAny<OrderedItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetCustomerOrders_NoOrders_ShouldEmptyList()
        {
            //Arrange
            DefaultSetup();

            //Act
            var orders = await repo.GetCustomerOrders(3);

            //Assert
            Assert.NotNull(orders);
            Assert.Equal(0, orders.Count);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderData.Order>()), Times.Never());
            mockOrders.Verify(m => m.Remove(It.IsAny<OrderData.Order>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderedItem>()), Times.Never());
            mockOrderedItems.Verify(m => m.Remove(It.IsAny<OrderedItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ProductsExist_AllExist_ShouldTrue()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.ProductsExist(productRepoModels);

            //Assert
            Assert.True(true == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ProductsExist_FirstDoesntExist_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            productRepoModels[0].ProductId = 99;

            //Act
            var result = await repo.ProductsExist(productRepoModels);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ProductsExist_SecondDoesntExist_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            productRepoModels[1].ProductId = 99;

            //Act
            var result = await repo.ProductsExist(productRepoModels);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ProductsExist_EmptyList_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            productRepoModels = new List<ProductRepoModel>();

            //Act
            var result = await repo.ProductsExist(productRepoModels);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ProductsExist_NullList_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            
            //Act
            var result = await repo.ProductsExist(null);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ProductsExist_FirstNull_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            productRepoModels[0] = null;

            //Act
            var result = await repo.ProductsExist(productRepoModels);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ProductsExist_SecondNull_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            productRepoModels[1] = null;

            //Act
            var result = await repo.ProductsExist(productRepoModels);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ProductsInStock_AllInStock_ShouldTrue()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.ProductsInStock(productRepoModels);

            //Assert
            Assert.True(true == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ProductsInStock_FirstNotInStock_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            productRepoModels[0].Quantity = 99;

            //Act
            var result = await repo.ProductsInStock(productRepoModels);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ProductsInStock_SecondNotInStock_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            productRepoModels[1].Quantity = 99;

            //Act
            var result = await repo.ProductsInStock(productRepoModels);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ProductsInStock_EmptyList_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            productRepoModels = new List<ProductRepoModel>();

            //Act
            var result = await repo.ProductsInStock(productRepoModels);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ProductsInStock_NullList_ShouldFalse()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await repo.ProductsInStock(null);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ProductsInStock_FirstNull_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            productRepoModels[0] = null;

            //Act
            var result = await repo.ProductsInStock(productRepoModels);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ProductsInStock_SecondNull_ShouldFalse()
        {
            //Arrange
            DefaultSetup();
            productRepoModels[1] = null;

            //Act
            var result = await repo.ProductsInStock(productRepoModels);

            //Assert
            Assert.True(false == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task CreateOrderSuccessfully()
        {
            //Arrange
            DefaultSetup();
            int product1OldStock = dbProduct1.Quantity;
            int product2OldStock = dbProduct2.Quantity;

            //Act
            var result = await repo.CreateOrder(finalOrder);

            //Assert
            Assert.True(0 == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderData.Order>()), Times.Once());
            mockOrders.Verify(m => m.Remove(It.IsAny<OrderData.Order>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderedItem>()), Times.Never());
            mockOrderedItems.Verify(m => m.Remove(It.IsAny<OrderedItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.Equal(product1OldStock - finalOrder.OrderedItems[0].Quantity, dbProduct1.Quantity);
            Assert.Equal(product2OldStock - finalOrder.OrderedItems[1].Quantity, dbProduct2.Quantity);
        }

        [Fact]
        public async Task CreateOrder_NullOrder_ReturnZero()
        {
            //Arrange
            DefaultSetup();
            int product1OldStock = dbProduct1.Quantity;
            int product2OldStock = dbProduct2.Quantity;

            //Act
            var result = await repo.CreateOrder(null);

            //Assert
            Assert.True(0 == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderData.Order>()), Times.Never());
            mockOrders.Verify(m => m.Remove(It.IsAny<OrderData.Order>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderedItem>()), Times.Never());
            mockOrderedItems.Verify(m => m.Remove(It.IsAny<OrderedItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
            Assert.Equal(product1OldStock, dbProduct1.Quantity);
            Assert.Equal(product2OldStock, dbProduct2.Quantity);
        }

        [Fact]
        public async Task CreateOrder_NullOrderedItems_ReturnZero()
        {
            //Arrange
            DefaultSetup();
            finalOrder.OrderedItems = null;
            int product1OldStock = dbProduct1.Quantity;
            int product2OldStock = dbProduct2.Quantity;

            //Act
            var result = await repo.CreateOrder(finalOrder);

            //Assert
            Assert.True(0 == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderData.Order>()), Times.Never());
            mockOrders.Verify(m => m.Remove(It.IsAny<OrderData.Order>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderedItem>()), Times.Never());
            mockOrderedItems.Verify(m => m.Remove(It.IsAny<OrderedItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
            Assert.Equal(product1OldStock, dbProduct1.Quantity);
            Assert.Equal(product2OldStock, dbProduct2.Quantity);
        }

        [Fact]
        public async Task CreateOrder_EmptyListOrderedItems_ReturnZero()
        {
            //Arrange
            DefaultSetup();
            finalOrder.OrderedItems = new List<OrderedItemRepoModel>();
            int product1OldStock = dbProduct1.Quantity;
            int product2OldStock = dbProduct2.Quantity;

            //Act
            var result = await repo.CreateOrder(finalOrder);

            //Assert
            Assert.True(0 == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderData.Order>()), Times.Never());
            mockOrders.Verify(m => m.Remove(It.IsAny<OrderData.Order>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderedItem>()), Times.Never());
            mockOrderedItems.Verify(m => m.Remove(It.IsAny<OrderedItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
            Assert.Equal(product1OldStock, dbProduct1.Quantity);
            Assert.Equal(product2OldStock, dbProduct2.Quantity);
        }

        [Fact]
        public async Task CreateOrder_FirstOrderedItemNull_ReturnZero()
        {
            //Arrange
            DefaultSetup();
            finalOrder.OrderedItems[0] = null;
            int product1OldStock = dbProduct1.Quantity;
            int product2OldStock = dbProduct2.Quantity;

            //Act
            var result = await repo.CreateOrder(finalOrder);

            //Assert
            Assert.True(0 == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderData.Order>()), Times.Never());
            mockOrders.Verify(m => m.Remove(It.IsAny<OrderData.Order>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderedItem>()), Times.Never());
            mockOrderedItems.Verify(m => m.Remove(It.IsAny<OrderedItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
            Assert.Equal(product1OldStock, dbProduct1.Quantity);
            Assert.Equal(product2OldStock, dbProduct2.Quantity);
        }

        [Fact]
        public async Task CreateOrder_SecondOrderedItemNull_ReturnZero()
        {
            //Arrange
            DefaultSetup();
            finalOrder.OrderedItems[1] = null;
            int product1OldStock = dbProduct1.Quantity;
            int product2OldStock = dbProduct2.Quantity;

            //Act
            var result = await repo.CreateOrder(finalOrder);

            //Assert
            Assert.True(0 == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderData.Order>()), Times.Never());
            mockOrders.Verify(m => m.Remove(It.IsAny<OrderData.Order>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderedItem>()), Times.Never());
            mockOrderedItems.Verify(m => m.Remove(It.IsAny<OrderedItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
            Assert.Equal(product1OldStock, dbProduct1.Quantity);
            Assert.Equal(product2OldStock, dbProduct2.Quantity);
        }

        [Fact]
        public async Task ClearBasket_OneItem_ShouldTrue()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;

            //Act
            var result = await repo.ClearBasket(customerId);

            //Assert
            Assert.True(true == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Once());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderData.Order>()), Times.Never());
            mockOrders.Verify(m => m.Remove(It.IsAny<OrderData.Order>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderedItem>()), Times.Never());
            mockOrderedItems.Verify(m => m.Remove(It.IsAny<OrderedItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task ClearBasket_TwoItems_ShouldTrue()
        {
            //Arrange
            DefaultSetup();
            dbBasketItem2.CustomerId = 1;
            int customerId = 1;

            //Act
            var result = await repo.ClearBasket(customerId);

            //Assert
            Assert.True(true == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Exactly(2));
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderData.Order>()), Times.Never());
            mockOrders.Verify(m => m.Remove(It.IsAny<OrderData.Order>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderedItem>()), Times.Never());
            mockOrderedItems.Verify(m => m.Remove(It.IsAny<OrderedItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task ClearBasket_NoItems_ShouldTrue()
        {
            //Arrange
            DefaultSetup();
            dbBasketItem1.CustomerId = 2;
            int customerId = 1;

            //Act
            var result = await repo.ClearBasket(customerId);

            //Assert
            Assert.True(true == result);
            mockDbContext.Verify(m => m.Add(It.IsAny<Customer>()), Times.Never());
            mockCustomers.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Never());
            mockBasketItems.Verify(m => m.Remove(It.IsAny<BasketItem>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<Product>()), Times.Never());
            mockProducts.Verify(m => m.Remove(It.IsAny<Product>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderData.Order>()), Times.Never());
            mockOrders.Verify(m => m.Remove(It.IsAny<OrderData.Order>()), Times.Never());
            mockDbContext.Verify(m => m.Add(It.IsAny<OrderedItem>()), Times.Never());
            mockOrderedItems.Verify(m => m.Remove(It.IsAny<OrderedItem>()), Times.Never());
            mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}

