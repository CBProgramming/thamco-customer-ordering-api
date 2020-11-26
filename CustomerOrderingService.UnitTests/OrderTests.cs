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

namespace CustomerOrderingService.UnitTests
{
    public class OrderTests
    {
        private List<OrderEFModel> setupStandardOrderEFModels()
        {
            return new List<OrderEFModel>()
            {
                new OrderEFModel {OrderId = 1, Date = new DateTime(2020,11,01), Total = 10.99 },
                new OrderEFModel {OrderId = 2, Date = new DateTime(2020,11,02), Total = 20.99 }
            };
        }

        private CustomerEFModel setupStandardCustomer()
        {
            return new CustomerEFModel
            {
                CustomerId = 1,
                Name = "Fake Name"
            };
        }

        private List<OrderedItemEFModel> setupStandardOrderedItemEFModels()
        {
            return new List<OrderedItemEFModel>()
            {
                new OrderedItemEFModel{OrderId = 1, ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = 2},
                new OrderedItemEFModel{OrderId = 2, ProductId = 1, Name = "Product 1", Price = 2.99, Quantity = 3},
                new OrderedItemEFModel{OrderId = 1, ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
        }

        private FakeOrderRepository setupFakeRepo(CustomerEFModel customer, List<OrderEFModel> orders, List<OrderedItemEFModel> orderedItems)
        {
            return new FakeOrderRepository
            {
                Customer = customer,
                Orders = orders,
                OrderedItems = orderedItems
            };
        }

        private IMapper setupMapper()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            }).CreateMapper();
        }

        private ILogger<OrderController> setupLogger()
        {
            return new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider()
                .GetService<ILoggerFactory>()
                .CreateLogger<OrderController>();
        }

        private List<OrderedItemDto> setupStandardOrderedItemDtos()
        {
            return new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
        }

        private List<ProductEFModel> setupStandardProductsInStock()
        {
            return new List<ProductEFModel>()
            {
                new ProductEFModel{ ProductId = 1, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 2, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 3, Name = "Fake", Quantity = 10}
            };
        }

        private FinalisedOrderDto setupStandardFinalisedOrderDto(List<OrderedItemDto> orderedItems)
        {
            return new FinalisedOrderDto
            {
                CustomerId = 1,
                Date = new DateTime(2020, 1, 1, 1, 1, 1, 1),
                OrderedItems = orderedItems,
                Total = 5.98
            };
        }

        private FakeOrderRepository setupFakeRepo(CustomerEFModel customer, List<ProductEFModel> productsInStock)
        {
            return new FakeOrderRepository
            {
                Customer = customer,
                Products = productsInStock
            };
        }

        private FakeOrderRepository setupStandardFakeRepo()
        {
            return new FakeOrderRepository
            {
                Customer = setupStandardCustomer(),
                Orders = setupStandardOrderEFModels(),
                OrderedItems = setupStandardOrderedItemEFModels(),
                Products = setupStandardProductsInStock()
            };
        }

        private OrderController setupStandardOrderController(FakeOrderRepository fakeRepo)
        {
            var mapper = setupMapper();
            var logger = setupLogger();
            var fakeFacade = new FakeStaffProductFacade();
            return new OrderController(logger, fakeRepo, mapper, fakeFacade);
        }

        [Fact]
        public async Task GetOrderHistory_ShouldOkObject()
        {
            //Arrange
            var fakeRepo = setupStandardFakeRepo();
            var controller = setupStandardOrderController(fakeRepo);
            var customerId = 1;

            //Act
            var result = await controller.Get(customerId,null);

            //Assert
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var historyResult = objResult.Value as List<OrderHistoryDto>;
            Assert.NotNull(historyResult);
            Assert.True(fakeRepo.Orders.Count == historyResult.Count);
            for (int i = 0; i < fakeRepo.Orders.Count; i++)
            {
                Assert.Equal(customerId, historyResult[i].CustomerId);
                Assert.Equal(fakeRepo.Orders[i].OrderId, historyResult[i].OrderId);
                Assert.Equal(fakeRepo.Orders[i].Date, historyResult[i].Date);
                Assert.Equal(fakeRepo.Orders[i].Total, historyResult[i].Total);
            }
        }

        [Fact]
        public async Task GetOrderHistory_ShouldNotFound()
        {
            //Arrange
            var customer = setupStandardCustomer();
            var orders = setupStandardOrderEFModels();
            var orderedItems = setupStandardOrderedItemEFModels();
            var fakeRepo = setupFakeRepo(customer, orders, orderedItems);
            var mapper = setupMapper();
            var logger = setupLogger();
            var fakeFacade = new FakeStaffProductFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);

            //Act
            var result = await controller.Get(2, null);

            //Assert
            Assert.NotNull(result);
            var notResult = result as NotFoundResult;
            Assert.NotNull(notResult);
        }

        [Fact]
        public async Task GetOrderHistory_NoOrders()
        {
            //Arrange
            var customer = setupStandardCustomer();
            var orderedItems = setupStandardOrderedItemEFModels();
            var fakeRepo = setupFakeRepo(customer, null, orderedItems);
            var mapper = setupMapper();
            var logger = setupLogger();
            var fakeFacade = new FakeStaffProductFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);

            //Act
            var result = await controller.Get(1, null);

            //Assert
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var historyResult = objResult.Value as List<OrderHistoryDto>;
            Assert.NotNull(historyResult);
            Assert.True(0 == historyResult.Count);
        }

        [Fact]
        public async Task CreateOrder_ShouldOk()
        {
            //Arrange
            var customer = setupStandardCustomer();
            var orderedItems = setupStandardOrderedItemDtos();
            var productsInStock = setupStandardProductsInStock();
            var finalisedOrder = setupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = setupFakeRepo(customer, productsInStock);
            var mapper = setupMapper();
            var logger = setupLogger();
            var fakeFacade = new FakeStaffProductFacade();

            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task CreateOrder_InvalidCustomerId_ShouldNotFound()
        {
            //Arrange
            var customer = setupStandardCustomer();
            var orderedItems = setupStandardOrderedItemDtos();
            var productsInStock = setupStandardProductsInStock();
            var finalisedOrder = setupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = setupFakeRepo(customer, productsInStock);
            var mapper = setupMapper();
            var logger = setupLogger();
            var fakeFacade = new FakeStaffProductFacade();
            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);
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
            var customer = setupStandardCustomer();
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = 2},
                new OrderedItemDto{ ProductId = 4, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
            var productsInStock = setupStandardProductsInStock();
            var finalisedOrder = setupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = setupFakeRepo(customer, productsInStock);
            var mapper = setupMapper();
            var logger = setupLogger();
            var fakeFacade = new FakeStaffProductFacade();

            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateOrder_OutOfStock_ShouldConflict()
        {
            //Arrange
            var customer = setupStandardCustomer();
            var orderedItems = setupStandardOrderedItemDtos();
            var productsInStock = new List<ProductEFModel>()
            {
                new ProductEFModel{ ProductId = 1, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 2, Name = "Fake", Quantity = 0},
                new ProductEFModel{ ProductId = 3, Name = "Fake", Quantity = 10}
            };
            var finalisedOrder = setupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = setupFakeRepo(customer, productsInStock);
            var mapper = setupMapper();
            var logger = setupLogger();
            var fakeFacade = new FakeStaffProductFacade();

            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<ConflictResult>(result);
        }

        [Fact]
        public async Task CreateOrder_NotEnoughStock_ShouldConflict()
        {
            //Arrange
            var customer = setupStandardCustomer();
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 15}
            };
            var productsInStock = setupStandardProductsInStock();
            var finalisedOrder = setupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = setupFakeRepo(customer, productsInStock);
            var mapper = setupMapper();
            var logger = setupLogger();
            var fakeFacade = new FakeStaffProductFacade();

            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<ConflictResult>(result);
        }

        [Fact]
        public async Task CreateOrder_NegativeQuantity_ShouldConflict()
        {
            //Arrange
            var customer = setupStandardCustomer();
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = -2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
            var productsInStock = setupStandardProductsInStock();
            var finalisedOrder = setupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = setupFakeRepo(customer, productsInStock);
            var mapper = setupMapper();
            var logger = setupLogger();
            var fakeFacade = new FakeStaffProductFacade();

            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);

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
            var customer = setupStandardCustomer();
            var orderedItems = setupStandardOrderedItemDtos();
            var productsInStock = setupStandardProductsInStock();
            var finalisedOrder = setupStandardFinalisedOrderDto(orderedItems);
            finalisedOrder.Date = new DateTime(2099, 1, 1, 1, 1, 1, 1);
            var fakeRepo = setupFakeRepo(customer, productsInStock);
            var mapper = setupMapper();
            var logger = setupLogger();
            var fakeFacade = new FakeStaffProductFacade();

            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.Equal(finalisedOrder.Date.Year, DateTime.Now.Year);
            Assert.Equal(finalisedOrder.Date.Month, DateTime.Now.Month);
            Assert.Equal(finalisedOrder.Date.Day, DateTime.Now.Day);
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
            var customer = setupStandardCustomer();
            var orderedItems = setupStandardOrderedItemDtos();
            var productsInStock = setupStandardProductsInStock();
            var finalisedOrder = setupStandardFinalisedOrderDto(orderedItems);
            finalisedOrder.Date = DateTime.Now.Subtract(TimeSpan.FromDays(7));
            var fakeRepo = setupFakeRepo(customer, productsInStock);
            var mapper = setupMapper();
            var logger = setupLogger();
            var fakeFacade = new FakeStaffProductFacade();

            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.Equal(finalisedOrder.Date.Year, DateTime.Now.Year);
            Assert.Equal(finalisedOrder.Date.Month, DateTime.Now.Month);
            Assert.Equal(finalisedOrder.Date.Day, DateTime.Now.Day);
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
            var customer = setupStandardCustomer();
            var orderedItems = setupStandardOrderedItemDtos();
            var productsInStock = setupStandardProductsInStock();
            var finalisedOrder = setupStandardFinalisedOrderDto(orderedItems);
            //set date two seconds before 7 day limit (any shorter a time and there's a risk of a correct failure)
            finalisedOrder.Date = DateTime.Now.Subtract(TimeSpan.FromDays(7)).Add(TimeSpan.FromSeconds(2));
            int year = finalisedOrder.Date.Year;
            int month = finalisedOrder.Date.Month;
            int day = finalisedOrder.Date.Day;
            int hour = finalisedOrder.Date.Hour;
            int minute = finalisedOrder.Date.Minute;
            int second = finalisedOrder.Date.Second;
            int millisecond = finalisedOrder.Date.Millisecond;
            var fakeRepo = setupFakeRepo(customer, productsInStock);
            var mapper = setupMapper();
            var logger = setupLogger();
            var fakeFacade = new FakeStaffProductFacade();

            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert date has not been adjusted significantly
            Assert.Equal(finalisedOrder.Date.Year, year);
            Assert.Equal(finalisedOrder.Date.Month, month);
            Assert.Equal(finalisedOrder.Date.Day, day);
            Assert.Equal(finalisedOrder.Date.Hour, hour);
            Assert.Equal(finalisedOrder.Date.Minute, minute);
            Assert.Equal(finalisedOrder.Date.Second, second);
            Assert.Equal(finalisedOrder.Date.Millisecond, millisecond);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task CreateOrder_NoOrderedItems()
        {
            //Arrange
            var customer = setupStandardCustomer();
            var orderedItems = new List<OrderedItemDto>()
            {
            };
            var productsInStock = setupStandardProductsInStock();
            var finalisedOrder = setupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = setupFakeRepo(customer, productsInStock);
            var mapper = setupMapper();
            var logger = setupLogger();
            var fakeFacade = new FakeStaffProductFacade();

            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<UnprocessableEntityResult>(result);
        }


        [Fact]
        public async Task CreateOrder_NullOrderedItems()
        {
            //Arrange
            var customer = setupStandardCustomer();
            var productsInStock = setupStandardProductsInStock();
            var finalisedOrder = setupStandardFinalisedOrderDto(null);
            var fakeRepo = setupFakeRepo(customer, productsInStock);
            var mapper = setupMapper();
            var logger = setupLogger();
            var fakeFacade = new FakeStaffProductFacade();

            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<UnprocessableEntityResult>(result);
        }

        [Fact]
        public async Task CreateOrder_ZeroUnitPrice_ShouldOk()
        {
            //Arrange
            var customer = setupStandardCustomer();
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 0, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
            var productsInStock = setupStandardProductsInStock();
            var finalisedOrder = setupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = setupFakeRepo(customer, productsInStock);
            var mapper = setupMapper();
            var logger = setupLogger();
            var fakeFacade = new FakeStaffProductFacade();

            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task CreateOrder_ZeroTotalPrice_ShouldOk()
        {
            //Arrange
            var customer = setupStandardCustomer();
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 0, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 0, Quantity = 5}
            };
            var productsInStock = setupStandardProductsInStock();
            var finalisedOrder = setupStandardFinalisedOrderDto(orderedItems);
            finalisedOrder.Total = 0;
            var fakeRepo = setupFakeRepo(customer, productsInStock);
            var mapper = setupMapper();
            var logger = setupLogger();
            var fakeFacade = new FakeStaffProductFacade();

            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task CreateOrder_NegativeUnitPrice_ShouldOk()
        {
            //Arrange
            var customer = setupStandardCustomer();
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = -0.01, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
            var productsInStock = setupStandardProductsInStock();
            var finalisedOrder = setupStandardFinalisedOrderDto(orderedItems);
            var fakeRepo = setupFakeRepo(customer, productsInStock);
            var mapper = setupMapper();
            var logger = setupLogger();
            var fakeFacade = new FakeStaffProductFacade();

            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<UnprocessableEntityResult>(result);
        }

        [Fact]
        public async Task CreateOrder_NegativeTotalPrice_ShouldOk()
        {
            //Arrange
            var customer = setupStandardCustomer();
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = -0.01, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = -0.01, Quantity = 5}
            };
            var productsInStock = setupStandardProductsInStock();
            var finalisedOrder = setupStandardFinalisedOrderDto(orderedItems);
            finalisedOrder.Total = -0.02;
            var fakeRepo = setupFakeRepo(customer, productsInStock);
            var mapper = setupMapper();
            var logger = setupLogger();
            var fakeFacade = new FakeStaffProductFacade();

            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<UnprocessableEntityResult>(result);
        }
    }
}
