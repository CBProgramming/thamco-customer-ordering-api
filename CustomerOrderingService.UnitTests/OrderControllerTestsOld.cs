using AutoMapper;
using CustomerOrderingService.Controllers;
using CustomerOrderingService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Order.Repository.Models;
using Order.Repository;
using StaffProduct.Facade;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Order.Repository.Data;
using Invoicing.Facade;

namespace CustomerOrderingService.UnitTests
{
    public class OrderControllerTestsOld
    {
        private List<OrderRepoModel> SetupStandardOrderEFModels()
        {
            return new List<OrderRepoModel>()
            {
                new OrderRepoModel {OrderId = 1, OrderDate = new DateTime(2020,11,01), Total = 10.99 },
                new OrderRepoModel {OrderId = 2, OrderDate = new DateTime(2020,11,02), Total = 20.99 }
            };
        }

        private CustomerRepoModel SetupStandardCustomer()
        {
            return new CustomerRepoModel
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

        private List<OrderedItemRepoModel> SetupStandardOrderedItemEFModels()
        {
            return new List<OrderedItemRepoModel>()
            {
                new OrderedItemRepoModel{OrderId = 1, ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = 2},
                new OrderedItemRepoModel{OrderId = 1, ProductId = 1, Name = "Product 1", Price = 2.99, Quantity = 3},
                new OrderedItemRepoModel{OrderId = 1, ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
        }

        private FakeOrderRepository SetupFakeRepo(CustomerRepoModel customer, List<OrderRepoModel> orders, List<OrderedItemRepoModel> orderedItems)
        {
            return new FakeOrderRepository
            {
                Customer = customer,
                Orders = orders,
                OrderedItems = orderedItems
            };
        }

        private IMapper SetupMapper()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            }).CreateMapper();
        }

        private ILogger<OrderController> SetupLogger()
        {
            return new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider()
                .GetService<ILoggerFactory>()
                .CreateLogger<OrderController>();
        }

        private List<OrderedItemDto> SetupStandardOrderedItemDtos()
        {
            return new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
        }

        private List<ProductRepoModel> SetupStandardProductsInStock()
        {
            return new List<ProductRepoModel>()
            {
                new ProductRepoModel{ ProductId = 1, Name = "Fake", Quantity = 10},
                new ProductRepoModel{ ProductId = 2, Name = "Fake", Quantity = 10},
                new ProductRepoModel{ ProductId = 3, Name = "Fake", Quantity = 10}
            };
        }

        private FinalisedOrderDto SetupStandardFinalisedOrderDto(List<OrderedItemDto> orderedItems)
        {
            return new FinalisedOrderDto
            {
                CustomerId = 1,
                OrderDate = new DateTime(2020, 1, 1, 1, 1, 1, 1),
                OrderedItems = orderedItems,
                Total = 5.98
            };
        }

        private FakeOrderRepository SetupFakeRepo(CustomerRepoModel customer, List<ProductRepoModel> productsInStock)
        {
            return new FakeOrderRepository
            {
                Customer = customer,
                Products = productsInStock
            };
        }

        [Fact]
        public async Task GetOrderHistory_ShouldOkObject()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orders = SetupStandardOrderEFModels();
            var orderedItems = SetupStandardOrderedItemEFModels();
            var fakeRepo = SetupFakeRepo(customer, orders, orderedItems);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);
            var customerId = 1;

            //Act
            var result = await controller.Get(customerId,null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var historyResult = objResult.Value as List<OrderHistoryDto>;
            Assert.NotNull(historyResult);
            Assert.True(fakeRepo.Orders.Count == historyResult.Count);
            for (int i = 0; i < fakeRepo.Orders.Count; i++)
            {
                Assert.Equal(customerId, historyResult[i].CustomerId);
                Assert.Equal(fakeRepo.Orders[i].OrderId, historyResult[i].OrderId);
                Assert.Equal(fakeRepo.Orders[i].OrderDate, historyResult[i].OrderDate);
                Assert.Equal(fakeRepo.Orders[i].Total, historyResult[i].Total);
            }
        }

        [Fact]
        public async Task GetOrderHistory_InvalidCustomerId_ShouldNotFound()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orders = SetupStandardOrderEFModels();
            var orderedItems = SetupStandardOrderedItemEFModels();
            var fakeRepo = SetupFakeRepo(customer, orders, orderedItems);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);

            //Act
            var result = await controller.Get(2, null);

            //Assert
            Assert.NotNull(result);
            var notResult = result as NotFoundResult;
            Assert.NotNull(notResult);
        }

        [Fact]
        public async Task GetOrderHistory_InactiveCustomerId_ShouldNotFound()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orders = SetupStandardOrderEFModels();
            var orderedItems = SetupStandardOrderedItemEFModels();
            var fakeRepo = SetupFakeRepo(customer, orders, orderedItems);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);

            customer.Active = false;

            //Act
            var result = await controller.Get(1, null);

            //Assert
            Assert.NotNull(result);
            var notResult = result as ForbidResult;
            Assert.NotNull(notResult);
        }

        [Fact]
        public async Task GetOrderHistory_NoOrders()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = SetupStandardOrderedItemEFModels();
            var fakeRepo = SetupFakeRepo(customer, null, orderedItems);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);

            //Act
            var result = await controller.Get(1, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var historyResult = objResult.Value as List<OrderHistoryDto>;
            Assert.NotNull(historyResult);
            Assert.True(0 == historyResult.Count);
        }

        [Fact]
        public async Task GetOrder_ShouldOkObject()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orders = SetupStandardOrderEFModels();
            var orderedItems = SetupStandardOrderedItemEFModels();
            var fakeRepo = SetupFakeRepo(customer, orders, orderedItems);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);
            int orderRequested = 1;

            //Act
            var result = await controller.Get(1, orderRequested);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var orderResult = objResult.Value as OrderDto;
            Assert.NotNull(orderResult);
            Assert.True(orders[orderRequested].OrderId == orderResult.OrderId);
            Assert.True(orders[orderRequested].OrderDate == orderResult.OrderDate);
            Assert.True(orders[orderRequested].Total == orderResult.Total);
            Assert.True(orderedItems.Count == orderResult.Products.Count);
            for (int i = 0; i < orderedItems.Count; i++)
            {
                Assert.True(orderedItems[i].OrderId == orderResult.Products[i].OrderId);
                Assert.True(orderedItems[i].Name == orderResult.Products[i].Name);
                Assert.True(orderedItems[i].ProductId == orderResult.Products[i].ProductId);
                Assert.True(orderedItems[i].Price == orderResult.Products[i].Price);
                Assert.True(orderedItems[i].Quantity == orderResult.Products[i].Quantity);
            }
        }

        [Fact]
        public async Task GetOrder_InvalidId_ShouldNotFound()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orders = SetupStandardOrderEFModels();
            var orderedItems = SetupStandardOrderedItemEFModels();
            var fakeRepo = SetupFakeRepo(customer, orders, orderedItems);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);
            int orderRequested = 99;

            //Act
            var result = await controller.Get(1, orderRequested);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async Task GetOrder_NoOrderedItems_ShouldOkObject()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orders = SetupStandardOrderEFModels();
            var fakeRepo = SetupFakeRepo(customer, orders, null);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);
            int orderRequested = 1;

            //Act
            var result = await controller.Get(1, orderRequested);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var orderResult = objResult.Value as OrderDto;
            Assert.NotNull(orderResult);
            Assert.True(orders[orderRequested].OrderId == orderResult.OrderId);
            Assert.True(orders[orderRequested].OrderDate == orderResult.OrderDate);
            Assert.True(orders[orderRequested].Total == orderResult.Total);
            Assert.True(0 == orderResult.Products.Count);
        }

        [Fact]
        public async Task CreateOrder_ShouldOk()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = SetupStandardOrderedItemDtos();
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task CreateOrder_NegativeItemPrice_ShouldUnprocessableEntity()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = SetupStandardOrderedItemDtos();
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);
            orderedItems[0].Price = -0.01;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<UnprocessableEntityResult>(result);
        }

        [Fact]
        public async Task CreateOrder_ZeroItemPrice_ShouldUnprocessableEntity()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = SetupStandardOrderedItemDtos();
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);
            orderedItems[0].Price = 0;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task CreateOrder_NegativeTotalPrice_ShouldUnprocessableEntity()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = SetupStandardOrderedItemDtos();
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);
            finalisedOrder.Total = -0.01;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<UnprocessableEntityResult>(result);
        }

        [Fact]
        public async Task CreateOrder_ZeroTotalPrice_ShouldUnprocessableEntity()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = SetupStandardOrderedItemDtos();
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);
            finalisedOrder.Total = 0;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task CreateOrder_NegativeItemQuantity_ShouldUnprocessableEntity()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = SetupStandardOrderedItemDtos();
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);
            orderedItems[0].Quantity = -1;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<UnprocessableEntityResult>(result);
        }

        [Fact]
        public async Task CreateOrder_ZeroItemQuantity_ShouldUnprocessableEntity()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = SetupStandardOrderedItemDtos();
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);
            orderedItems[0].Quantity = 0;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<UnprocessableEntityResult>(result);
        }

        [Fact]
        public async Task CreateOrder_InvalidCustomerId_ShouldNotFound()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = SetupStandardOrderedItemDtos();
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);
            //set invalid customer Id
            finalisedOrder.CustomerId = 2;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateOrder_InvalidProductId_ShouldNotFound()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = 2},
                new OrderedItemDto{ ProductId = 4, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateOrder_CustomerCantPurchase_ShouldForbid()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = SetupStandardOrderedItemDtos();
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);
            customer.CanPurchase = false;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task CreateOrder_CustomerNotActive_ShouldForbid()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = SetupStandardOrderedItemDtos();
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);
            customer.Active = false;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task CreateOrder_OutOfStock_ShouldConflict()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = SetupStandardOrderedItemDtos();
            var productsInStock = new List<ProductRepoModel>()
            {
                new ProductRepoModel{ ProductId = 1, Name = "Fake", Quantity = 10},
                new ProductRepoModel{ ProductId = 2, Name = "Fake", Quantity = 0},
                new ProductRepoModel{ ProductId = 3, Name = "Fake", Quantity = 10}
            };
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<ConflictResult>(result);
        }

        [Fact]
        public async Task CreateOrder_NotEnoughStock_ShouldConflict()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 15}
            };
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<ConflictResult>(result);
        }

        [Fact]
        public async Task CreateOrder_NegativeQuantity_ShouldConflict()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = -2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<UnprocessableEntityResult>(result);
        }

        [Fact]
        public async Task CreateOrder_FutureDate_ShouldOkWithTodaysDate()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if(DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = SetupStandardOrderedItemDtos();
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            finalisedOrder.OrderDate = new DateTime(2099, 1, 1, 1, 1, 1, 1);
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.Equal(finalisedOrder.OrderDate.Year, DateTime.Now.Year);
            Assert.Equal(finalisedOrder.OrderDate.Month, DateTime.Now.Month);
            Assert.Equal(finalisedOrder.OrderDate.Day, DateTime.Now.Day);
            Assert.IsType<OkResult>(result);
        }


        [Fact]
        public async Task CreateOrder_DateSevenDaysAgoExactly_ShouldOkWithTodaysDate()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = SetupStandardOrderedItemDtos();
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            finalisedOrder.OrderDate = DateTime.Now.Subtract(TimeSpan.FromDays(7));
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.Equal(finalisedOrder.OrderDate.Year, DateTime.Now.Year);
            Assert.Equal(finalisedOrder.OrderDate.Month, DateTime.Now.Month);
            Assert.Equal(finalisedOrder.OrderDate.Day, DateTime.Now.Day);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task CreateOrder_AlmostSevenDaysAgo_ShouldOkWithOriginalDate()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = SetupStandardOrderedItemDtos();
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            //set date two seconds before 7 day limit (any shorter a time and there's a risk of a correct failure)
            finalisedOrder.OrderDate = DateTime.Now.Subtract(TimeSpan.FromDays(7)).Add(TimeSpan.FromSeconds(2));
            int year = finalisedOrder.OrderDate.Year;
            int month = finalisedOrder.OrderDate.Month;
            int day = finalisedOrder.OrderDate.Day;
            int hour = finalisedOrder.OrderDate.Hour;
            int minute = finalisedOrder.OrderDate.Minute;
            int second = finalisedOrder.OrderDate.Second;
            int millisecond = finalisedOrder.OrderDate.Millisecond;
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert date has not been adjusted significantly
            Assert.Equal(finalisedOrder.OrderDate.Year, year);
            Assert.Equal(finalisedOrder.OrderDate.Month, month);
            Assert.Equal(finalisedOrder.OrderDate.Day, day);
            Assert.Equal(finalisedOrder.OrderDate.Hour, hour);
            Assert.Equal(finalisedOrder.OrderDate.Minute, minute);
            Assert.Equal(finalisedOrder.OrderDate.Second, second);
            Assert.Equal(finalisedOrder.OrderDate.Millisecond, millisecond);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task CreateOrder_NoOrderedItems()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = new List<OrderedItemDto>()
            {
            };
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<UnprocessableEntityResult>(result);
        }


        [Fact]
        public async Task CreateOrder_NullOrderedItems()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(null);
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<UnprocessableEntityResult>(result);
        }

        [Fact]
        public async Task CreateOrder_ZeroUnitPrice_ShouldOk()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 0, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task CreateOrder_ZeroTotalPrice_ShouldOk()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 0, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 0, Quantity = 5}
            };
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            finalisedOrder.Total = 0;
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task CreateOrder_NegativeUnitPrice_ShouldOk()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = -0.01, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<UnprocessableEntityResult>(result);
        }

        [Fact]
        public async Task CreateOrder_NegativeTotalPrice_ShouldOk()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = -0.01, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = -0.01, Quantity = 5}
            };
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            finalisedOrder.Total = -0.02;
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<UnprocessableEntityResult>(result);
        }

        [Fact]
        public async Task CreateOrder_RepoFailure_ShouldNotFound()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = SetupStandardOrderedItemDtos();
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);
            fakeRepo.CompletesOrders = false;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateOrder_FacadeFailure_ShouldNotFound()
        {
            //Arrange
            var customer = SetupStandardCustomer();
            var orderedItems = SetupStandardOrderedItemDtos();
            var productsInStock = SetupStandardProductsInStock();
            var finalisedOrder = SetupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = SetupFakeRepo(customer, productsInStock);
            var mapper = SetupMapper();
            var logger = SetupLogger();
            var fakeProductFacade = new FakeStaffProductFacade();
            var fakeInvoiceFacade = new FakeInvoiceFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);
            fakeProductFacade.CompletesStockReduction = false;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
