using AutoMapper;
using CustomerOrderingService.Controllers;
using CustomerOrderingService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Order.Repository;
using Order.Repository.Data;
using Order.Repository.Models;
using StaffProduct.Facade;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CustomerOrderingService.UnitTests
{
    public class BasketTests
    {
        private CustomerEFModel SetupStandardCustomer()
        {
            return new CustomerEFModel
            {
                CustomerId = 1,
                GivenName = "Fake",
                FamilyName = "Name",
                AddressOne = "Address 1",
                AddressTwo = "Address 2",
                Town = "Town",
                State = "State",
                AreaCode = "Area Code",
                TelephoneNumber = "Telephone Number",
                CanPurchase = true,
                Active = true
            };
        }

        private BasketItemDto SetupStandardBasketItem()
        {
            return new BasketItemDto
            {
                CustomerId = 1,
                ProductId = 1,
                Price = 1.00,
                Quantity = 1,
                ProductName = "Fake name"
            };
        }



        private FakeOrderRepository SetupFakeRepo(CustomerEFModel customer, List<BasketProductsEFModel>? currentBasket,
            List<ProductEFModel> products)
        {
            return new FakeOrderRepository
            {
                Customer = customer,
                CurrentBasket = currentBasket,
                Products = products
            };
        }

        private IMapper SetupMapper()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            }).CreateMapper();
        }

        private ILogger<BasketController> SetupLogger()
        {
            return new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider()
                .GetService<ILoggerFactory>()
                .CreateLogger<BasketController>();
        }

        private List<BasketProductsEFModel> SetupProductsInBasket()
        {
            return new List<BasketProductsEFModel>()
            {
                new BasketProductsEFModel{ ProductId = 1, ProductName = "Product1", Price = 1.99, Quantity = 2},
                new BasketProductsEFModel{ ProductId = 2, ProductName = "Product2", Price = 2.00, Quantity = 3},
                new BasketProductsEFModel{ ProductId = 3, ProductName = "Product3", Price = 3.00, Quantity = 4},
                new BasketProductsEFModel{ ProductId = 4, ProductName = "Product4", Price = 4.00, Quantity = 10}
            };
        }

        private List<ProductEFModel> SetupStandardProductsInStock()
        {
            return new List<ProductEFModel>()
            {
                new ProductEFModel{ ProductId = 1, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 2, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 3, Name = "Fake", Quantity = 10}
            };
        }

        [Fact]
        public async Task GetBasket_ShouldOkObject()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var currentBasket = SetupProductsInBasket();
            var products = SetupStandardProductsInStock();
            var fakeRepo = SetupFakeRepo(customer, currentBasket, products);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var controller = new BasketController(logger, fakeRepo, mapper);
            var customerId = 1;

            //Act
            var result = await controller.Get(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var basketItems = objResult.Value as List<BasketItemDto>;
            Assert.NotNull(basketItems);
            Assert.True(fakeRepo.CurrentBasket.Count == basketItems.Count);
            for (int i = 0; i < fakeRepo.CurrentBasket.Count; i++)
            {
                Assert.Equal(fakeRepo.CurrentBasket[i].ProductId, currentBasket[i].ProductId);
                Assert.Equal(fakeRepo.CurrentBasket[i].ProductName, currentBasket[i].ProductName);
                Assert.Equal(fakeRepo.CurrentBasket[i].Price, currentBasket[i].Price);
                Assert.Equal(fakeRepo.CurrentBasket[i].Quantity, currentBasket[i].Quantity);
            }
        }

        [Fact]
        public async Task GetEmptyBasket_ShouldOkObject()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var products = SetupStandardProductsInStock();
            var fakeRepo = SetupFakeRepo(customer, null, products);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var controller = new BasketController(logger, fakeRepo, mapper);
            var customerId = 1;

            //Act
            var result = await controller.Get(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var basketItems = objResult.Value as List<BasketItemDto>;
            Assert.NotNull(basketItems);
            Assert.True(0 == basketItems.Count);
        }

        [Fact]
        public async Task GetBasket_InvalidCustomer_ShouldNotFound()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var products = SetupStandardProductsInStock();
            var currentBasket = SetupProductsInBasket();
            var fakeRepo = SetupFakeRepo(customer, currentBasket, products);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var controller = new BasketController(logger, fakeRepo, mapper);
            var customerId = 2;

            //Act
            var result = await controller.Get(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async Task GetBasket_InactiveCustomer_ShouldForbid()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var currentBasket = SetupProductsInBasket();
            var products = SetupStandardProductsInStock();
            var fakeRepo = SetupFakeRepo(customer, currentBasket, products);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var controller = new BasketController(logger, fakeRepo, mapper);
            var customerId = 1;
            customer.Active = false;

            //Act
            var result = await controller.Get(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async Task NewBasketItem_ShouldOk()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var currentBasket = SetupProductsInBasket();
            var products = SetupStandardProductsInStock();
            var fakeRepo = SetupFakeRepo(customer, currentBasket, products);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var basketItem = SetupStandardBasketItem();
            var controller = new BasketController(logger, fakeRepo, mapper);
            var customerId = 1;

            //Act
            var result = await controller.Create(basketItem);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async Task NewBasketItem_InvalidCustomer_ShouldNotFound()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var currentBasket = SetupProductsInBasket();
            var products = SetupStandardProductsInStock();
            var fakeRepo = SetupFakeRepo(customer, currentBasket, products);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var basketItem = SetupStandardBasketItem();
            var controller = new BasketController(logger, fakeRepo, mapper);
            basketItem.CustomerId = 2;

            //Act
            var result = await controller.Create(basketItem);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async Task NewBasketItem_InactiveCustomer_ShouldNotFound()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var products = SetupStandardProductsInStock();
            var currentBasket = SetupProductsInBasket();
            var fakeRepo = SetupFakeRepo(customer, currentBasket, products);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var basketItem = SetupStandardBasketItem();
            var controller = new BasketController(logger, fakeRepo, mapper);
            customer.Active = false;

            //Act
            var result = await controller.Create(basketItem);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async Task NewBasketItem_InvalidProductId_ShouldNotFound()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var products = SetupStandardProductsInStock();
            var currentBasket = SetupProductsInBasket();
            var fakeRepo = SetupFakeRepo(customer, currentBasket, products);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var basketItem = SetupStandardBasketItem();
            var controller = new BasketController(logger, fakeRepo, mapper);
            basketItem.ProductId = 99;

            //Act
            var result = await controller.Create(basketItem);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async Task NewBasketItem_InvalidProductName_ShouldShouldUnprocessableEntity()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var products = SetupStandardProductsInStock();
            var currentBasket = SetupProductsInBasket();
            var fakeRepo = SetupFakeRepo(customer, currentBasket, products);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var basketItem = SetupStandardBasketItem();
            var controller = new BasketController(logger, fakeRepo, mapper);
            basketItem.ProductName = null;

            //Act
            var result = await controller.Create(basketItem);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async Task NewBasketItem_ZeroPrice_ShouldOk()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var products = SetupStandardProductsInStock();
            var currentBasket = SetupProductsInBasket();
            var fakeRepo = SetupFakeRepo(customer, currentBasket, products);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var basketItem = SetupStandardBasketItem();
            var controller = new BasketController(logger, fakeRepo, mapper);
            basketItem.Price = 0;

            //Act
            var result = await controller.Create(basketItem);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async Task NewBasketItem_NegativePrice_ShouldShouldUnprocessableEntity()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var products = SetupStandardProductsInStock();
            var currentBasket = SetupProductsInBasket();
            var fakeRepo = SetupFakeRepo(customer, currentBasket, products);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var basketItem = SetupStandardBasketItem();
            var controller = new BasketController(logger, fakeRepo, mapper);
            basketItem.Price = -0.01;

            //Act
            var result = await controller.Create(basketItem);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async Task NewBasketItem_ZeroQuantity_ShouldUnprocessableEntity()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var products = SetupStandardProductsInStock();
            var currentBasket = SetupProductsInBasket();
            var fakeRepo = SetupFakeRepo(customer, currentBasket, products);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var basketItem = SetupStandardBasketItem();
            var controller = new BasketController(logger, fakeRepo, mapper);
            basketItem.Quantity = 0;

            //Act
            var result = await controller.Create(basketItem);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async Task NewBasketItem_NegativeQuantity_ShouldUnprocessableEntity()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var products = SetupStandardProductsInStock();
            var currentBasket = SetupProductsInBasket();
            var fakeRepo = SetupFakeRepo(customer, currentBasket, products);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var basketItem = SetupStandardBasketItem();
            var controller = new BasketController(logger, fakeRepo, mapper);
            basketItem.Quantity = -1;

            //Act
            var result = await controller.Create(basketItem);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }
    }
}
