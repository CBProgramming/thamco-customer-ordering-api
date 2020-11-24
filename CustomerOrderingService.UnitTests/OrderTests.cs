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
        [Fact]
        public async Task GetOrderHistory_ShouldOkObject()
        {
            //Arrange
            var customer = new CustomerEFModel
            {
                CustomerId = 1,
                Name = "Fake Name"
            };
            var orders = new List<OrderEFModel>()
            {
                new OrderEFModel {Id = 1, OrderDate = new DateTime(2020,11,01), Total = 10.99 },
                new OrderEFModel {Id = 2, OrderDate = new DateTime(2020,11,02), Total = 20.99 }
            };
            var orderedItems = new List<OrderedItemEFModel>()
            {
                new OrderedItemEFModel{OrderId = 1, ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = 2},
                new OrderedItemEFModel{OrderId = 2, ProductId = 1, Name = "Product 1", Price = 2.99, Quantity = 3},
                new OrderedItemEFModel{OrderId = 1, ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
            var fakeRepo = new FakeOrderRepository
            {
                Customer = customer,
                Orders = orders,
                OrderedItems = orderedItems
            };
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            });
            var mapper = mockMapper.CreateMapper();
            var sp = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var logFactory = sp.GetService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<OrderController>();
            var fakeFacade = new FakeStaffProductFacade();

            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);

            //Act
            var result = await controller.Get(1);

            //Assert
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var historyResult = objResult.Value as OrderHistoryDto;
            Assert.NotNull(historyResult);
            var customerResult = historyResult.Customer;
            Assert.Equal(customerResult.Name, customer.Name);
            Assert.Equal(customerResult.CustomerId, customer.CustomerId);
            var ordersResult = historyResult.Orders as List<OrderDto>;
            Assert.True(orders.Count == ordersResult.Count);
            for (int i = 0; i < orders.Count; i++)
            {
                Assert.Equal(orders[i].Id, ordersResult[i].Id);
                Assert.Equal(orders[i].OrderDate, ordersResult[i].OrderDate);
                Assert.Equal(orders[i].Total, ordersResult[i].Total);
                Assert.True(orderedItems.Count == ordersResult[i].Products.Count);
                for (int j = 0; j < orderedItems.Count; j++)
                {
                    Assert.Equal(orderedItems[j].Name, ordersResult[i].Products[j].Name);
                    Assert.Equal(orderedItems[j].OrderId, ordersResult[i].Products[j].OrderId);
                    Assert.Equal(orderedItems[j].Price, ordersResult[i].Products[j].Price);
                    Assert.Equal(orderedItems[j].ProductId, ordersResult[i].Products[j].ProductId);
                    Assert.Equal(orderedItems[j].Quantity, ordersResult[i].Products[j].Quantity);
                }
            }
        }

        [Fact]
        public async Task GetOrderHistory_ShouldNotFound()
        {
            //Arrange
            var customer = new CustomerEFModel
            {
                CustomerId = 1,
                Name = "Fake Name"
            };
            var orders = new List<OrderEFModel>()
            {
                new OrderEFModel {Id = 1, OrderDate = new DateTime(2020,11,01), Total = 10.99 },
                new OrderEFModel {Id = 2, OrderDate = new DateTime(2020,11,02), Total = 20.99 }
            };
            var orderedItems = new List<OrderedItemEFModel>()
            {
                new OrderedItemEFModel{OrderId = 1, ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = 2},
                new OrderedItemEFModel{OrderId = 2, ProductId = 1, Name = "Product 1", Price = 2.99, Quantity = 3},
                new OrderedItemEFModel{OrderId = 1, ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
            var fakeRepo = new FakeOrderRepository
            {
                Customer = customer,
                Orders = orders,
                OrderedItems = orderedItems
            };
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            });
            var mapper = mockMapper.CreateMapper();
            var sp = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var logFactory = sp.GetService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<OrderController>();
            var fakeFacade = new FakeStaffProductFacade();

            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);

            //Act
            var result = await controller.Get(2);

            //Assert
            Assert.NotNull(result);
            var notResult = result as NotFoundResult;
            Assert.NotNull(notResult);
        }

        [Fact]
        public async Task GetOrderHistory_NoOrders()
        {
            //Arrange
            var customer = new CustomerEFModel
            {
                CustomerId = 1,
                Name = "Fake Name"
            };
            var orderedItems = new List<OrderedItemEFModel>()
            {
                new OrderedItemEFModel{OrderId = 1, ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = 2},
                new OrderedItemEFModel{OrderId = 2, ProductId = 1, Name = "Product 1", Price = 2.99, Quantity = 3},
                new OrderedItemEFModel{OrderId = 1, ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
            var fakeRepo = new FakeOrderRepository
            {
                Customer = customer,
                Orders = null,
                OrderedItems = orderedItems
            };
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            });
            var mapper = mockMapper.CreateMapper();
            var sp = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var logFactory = sp.GetService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<OrderController>();
            var fakeFacade = new FakeStaffProductFacade();

            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);

            //Act
            var result = await controller.Get(1);

            //Assert
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var historyResult = objResult.Value as OrderHistoryDto;
            Assert.NotNull(historyResult);
            var customerResult = historyResult.Customer;
            Assert.Equal(customerResult.Name, customer.Name);
            Assert.Equal(customerResult.CustomerId, customer.CustomerId);
            var ordersResult = historyResult.Orders as List<OrderDto>;
            Assert.True(0 == ordersResult.Count);
        }

        [Fact]
        public async Task CreateOrder_ShouldOk()
        {
            //Arrange
            var customer = new CustomerEFModel
            {
                CustomerId = 1,
                Name = "Fake Name"
            };
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
            var productsInStock = new List<ProductEFModel>()
            {
                new ProductEFModel{ ProductId = 1, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 2, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 3, Name = "Fake", Quantity = 10}
            };
            var finalisedOrder = new FinalisedOrderDto
            {
                CustomerId = 1,
                Date = new DateTime(2020, 1, 1, 1, 1, 1, 1),
                OrderedItems = orderedItems,
                Total = 5.98
            };
            var fakeRepo = new FakeOrderRepository
            {
                Customer = customer,
                Products = productsInStock
            };

            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            });
            var mapper = mockMapper.CreateMapper();

            var sp = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();
            var logFactory = sp.GetService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<OrderController>();

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
            var customer = new CustomerEFModel
            {
                CustomerId = 2,
                Name = "Fake Name"
            };
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
            var productsInStock = new List<ProductEFModel>()
            {
                new ProductEFModel{ ProductId = 1, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 2, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 3, Name = "Fake", Quantity = 10}
            };
            var finalisedOrder = new FinalisedOrderDto
            {
                CustomerId = 1,
                Date = new DateTime(2020, 1, 1, 1, 1, 1, 1),
                OrderedItems = orderedItems,
                Total = 5.98
            };
            var fakeRepo = new FakeOrderRepository
            {
                Customer = customer,
                Products = productsInStock
            };

            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            });
            var mapper = mockMapper.CreateMapper();

            var sp = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();
            var logFactory = sp.GetService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<OrderController>();

            var fakeFacade = new FakeStaffProductFacade();

            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateOrder_InvalidProductId_ShouldNotFound()
        {
            //Arrange
            var customer = new CustomerEFModel
            {
                CustomerId = 1,
                Name = "Fake Name"
            };
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = 2},
                new OrderedItemDto{ ProductId = 4, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
            var productsInStock = new List<ProductEFModel>()
            {
                new ProductEFModel{ ProductId = 1, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 2, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 3, Name = "Fake", Quantity = 10}
            };
            var finalisedOrder = new FinalisedOrderDto
            {
                CustomerId = 1,
                Date = new DateTime(2020, 1, 1, 1, 1, 1, 1),
                OrderedItems = orderedItems,
                Total = 5.98
            };
            var fakeRepo = new FakeOrderRepository
            {
                Customer = customer,
                Products = productsInStock
            };

            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            });
            var mapper = mockMapper.CreateMapper();

            var sp = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();
            var logFactory = sp.GetService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<OrderController>();

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
            var customer = new CustomerEFModel
            {
                CustomerId = 1,
                Name = "Fake Name"
            };
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
            var productsInStock = new List<ProductEFModel>()
            {
                new ProductEFModel{ ProductId = 1, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 2, Name = "Fake", Quantity = 0},
                new ProductEFModel{ ProductId = 3, Name = "Fake", Quantity = 10}
            };
            var finalisedOrder = new FinalisedOrderDto
            {
                CustomerId = 1,
                Date = new DateTime(2020, 1, 1, 1, 1, 1, 1),
                OrderedItems = orderedItems,
                Total = 5.98
            };
            var fakeRepo = new FakeOrderRepository
            {
                Customer = customer,
                Products = productsInStock
            };

            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            });
            var mapper = mockMapper.CreateMapper();

            var sp = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();
            var logFactory = sp.GetService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<OrderController>();

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
            var customer = new CustomerEFModel
            {
                CustomerId = 1,
                Name = "Fake Name"
            };
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 10}
            };
            var productsInStock = new List<ProductEFModel>()
            {
                new ProductEFModel{ ProductId = 1, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 2, Name = "Fake", Quantity = 5},
                new ProductEFModel{ ProductId = 3, Name = "Fake", Quantity = 10}
            };
            var finalisedOrder = new FinalisedOrderDto
            {
                CustomerId = 1,
                Date = new DateTime(2020, 1, 1, 1, 1, 1, 1),
                OrderedItems = orderedItems,
                Total = 5.98
            };
            var fakeRepo = new FakeOrderRepository
            {
                Customer = customer,
                Products = productsInStock
            };

            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            });
            var mapper = mockMapper.CreateMapper();

            var sp = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();
            var logFactory = sp.GetService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<OrderController>();

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
            var customer = new CustomerEFModel
            {
                CustomerId = 1,
                Name = "Fake Name"
            };
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = -2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
            var productsInStock = new List<ProductEFModel>()
            {
                new ProductEFModel{ ProductId = 1, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 2, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 3, Name = "Fake", Quantity = 10}
            };
            var finalisedOrder = new FinalisedOrderDto
            {
                CustomerId = 1,
                Date = new DateTime(2020, 1, 1, 1, 1, 1, 1),
                OrderedItems = orderedItems,
                Total = 5.98
            };
            var fakeRepo = new FakeOrderRepository
            {
                Customer = customer,
                Products = productsInStock
            };

            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            });
            var mapper = mockMapper.CreateMapper();

            var sp = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();
            var logFactory = sp.GetService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<OrderController>();

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
            var customer = new CustomerEFModel
            {
                CustomerId = 1,
                Name = "Fake Name"
            };
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
            var productsInStock = new List<ProductEFModel>()
            {
                new ProductEFModel{ ProductId = 1, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 2, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 3, Name = "Fake", Quantity = 10}
            };
            var finalisedOrder = new FinalisedOrderDto
            {
                CustomerId = 1,
                Date = new DateTime(2099, 1, 1, 1, 1, 1, 1),
                OrderedItems = orderedItems,
                Total = 5.98
            };
            var fakeRepo = new FakeOrderRepository
            {
                Customer = customer,
                Products = productsInStock
            };

            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            });
            var mapper = mockMapper.CreateMapper();

            var sp = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();
            var logFactory = sp.GetService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<OrderController>();

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
            var customer = new CustomerEFModel
            {
                CustomerId = 1,
                Name = "Fake Name"
            };
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
            var productsInStock = new List<ProductEFModel>()
            {
                new ProductEFModel{ ProductId = 1, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 2, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 3, Name = "Fake", Quantity = 10}
            };
            var finalisedOrder = new FinalisedOrderDto
            {
                CustomerId = 1,
                Date = DateTime.Now.Subtract(TimeSpan.FromDays(7)),
                OrderedItems = orderedItems,
                Total = 5.98
            };
            var fakeRepo = new FakeOrderRepository
            {
                Customer = customer,
                Products = productsInStock
            };

            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            });
            var mapper = mockMapper.CreateMapper();

            var sp = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();
            var logFactory = sp.GetService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<OrderController>();

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
            var customer = new CustomerEFModel
            {
                CustomerId = 1,
                Name = "Fake Name"
            };
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
            var productsInStock = new List<ProductEFModel>()
            {
                new ProductEFModel{ ProductId = 1, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 2, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 3, Name = "Fake", Quantity = 10}
            };
            var finalisedOrder = new FinalisedOrderDto
            {
                CustomerId = 1,
                //set date two seconds before 7 day limit (any shorter a time and there's a risk of a correct failure)
                Date = DateTime.Now.Subtract(TimeSpan.FromDays(7)).Add(TimeSpan.FromSeconds(2)),
                OrderedItems = orderedItems,
                Total = 5.98
            };
            int year = finalisedOrder.Date.Year;
            int month = finalisedOrder.Date.Month;
            int day = finalisedOrder.Date.Day;
            int hour = finalisedOrder.Date.Hour;
            int minute = finalisedOrder.Date.Minute;
            int second = finalisedOrder.Date.Second;
            int millisecond = finalisedOrder.Date.Millisecond;
            var fakeRepo = new FakeOrderRepository
            {
                Customer = customer,
                Products = productsInStock
            };

            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            });
            var mapper = mockMapper.CreateMapper();

            var sp = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();
            var logFactory = sp.GetService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<OrderController>();

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
            var customer = new CustomerEFModel
            {
                CustomerId = 1,
                Name = "Fake Name"
            };
            var orderedItems = new List<OrderedItemDto>()
            {
            };
            var productsInStock = new List<ProductEFModel>()
            {
                new ProductEFModel{ ProductId = 1, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 2, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 3, Name = "Fake", Quantity = 10}
            };
            var finalisedOrder = new FinalisedOrderDto
            {
                CustomerId = 1,
                Date = new DateTime(2020, 1, 1, 1, 1, 1, 1),
                OrderedItems = orderedItems,
                Total = 5.98
            };
            var fakeRepo = new FakeOrderRepository
            {
                Customer = customer,
                Products = productsInStock
            };

            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            });
            var mapper = mockMapper.CreateMapper();

            var sp = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();
            var logFactory = sp.GetService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<OrderController>();

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
            var customer = new CustomerEFModel
            {
                CustomerId = 1,
                Name = "Fake Name"
            };
            var productsInStock = new List<ProductEFModel>()
            {
                new ProductEFModel{ ProductId = 1, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 2, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 3, Name = "Fake", Quantity = 10}
            };
            var finalisedOrder = new FinalisedOrderDto
            {
                CustomerId = 1,
                Date = new DateTime(2020, 1, 1, 1, 1, 1, 1),
                OrderedItems = null,
                Total = 5.98
            };
            var fakeRepo = new FakeOrderRepository
            {
                Customer = customer,
                Products = productsInStock
            };

            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            });
            var mapper = mockMapper.CreateMapper();

            var sp = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();
            var logFactory = sp.GetService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<OrderController>();

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
            var customer = new CustomerEFModel
            {
                CustomerId = 1,
                Name = "Fake Name"
            };
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 0, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
            var productsInStock = new List<ProductEFModel>()
            {
                new ProductEFModel{ ProductId = 1, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 2, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 3, Name = "Fake", Quantity = 10}
            };
            var finalisedOrder = new FinalisedOrderDto
            {
                CustomerId = 1,
                Date = new DateTime(2020, 1, 1, 1, 1, 1, 1),
                OrderedItems = orderedItems,
                Total = 3.99
            };
            var fakeRepo = new FakeOrderRepository
            {
                Customer = customer,
                Products = productsInStock
            };

            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            });
            var mapper = mockMapper.CreateMapper();

            var sp = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();
            var logFactory = sp.GetService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<OrderController>();

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
            var customer = new CustomerEFModel
            {
                CustomerId = 1,
                Name = "Fake Name"
            };
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 0, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 0, Quantity = 5}
            };
            var productsInStock = new List<ProductEFModel>()
            {
                new ProductEFModel{ ProductId = 1, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 2, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 3, Name = "Fake", Quantity = 10}
            };
            var finalisedOrder = new FinalisedOrderDto
            {
                CustomerId = 1,
                Date = new DateTime(2020, 1, 1, 1, 1, 1, 1),
                OrderedItems = orderedItems,
                Total = 0
            };
            var fakeRepo = new FakeOrderRepository
            {
                Customer = customer,
                Products = productsInStock
            };

            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            });
            var mapper = mockMapper.CreateMapper();

            var sp = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();
            var logFactory = sp.GetService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<OrderController>();

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
            var customer = new CustomerEFModel
            {
                CustomerId = 1,
                Name = "Fake Name"
            };
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = -0.01, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
            var productsInStock = new List<ProductEFModel>()
            {
                new ProductEFModel{ ProductId = 1, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 2, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 3, Name = "Fake", Quantity = 10}
            };
            var finalisedOrder = new FinalisedOrderDto
            {
                CustomerId = 1,
                Date = new DateTime(2020, 1, 1, 1, 1, 1, 1),
                OrderedItems = orderedItems,
                Total = 3.98
            };
            var fakeRepo = new FakeOrderRepository
            {
                Customer = customer,
                Products = productsInStock
            };

            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            });
            var mapper = mockMapper.CreateMapper();

            var sp = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();
            var logFactory = sp.GetService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<OrderController>();

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
            var customer = new CustomerEFModel
            {
                CustomerId = 1,
                Name = "Fake Name"
            };
            var orderedItems = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = -0.01, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = -0.01, Quantity = 5}
            };
            var productsInStock = new List<ProductEFModel>()
            {
                new ProductEFModel{ ProductId = 1, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 2, Name = "Fake", Quantity = 10},
                new ProductEFModel{ ProductId = 3, Name = "Fake", Quantity = 10}
            };
            var finalisedOrder = new FinalisedOrderDto
            {
                CustomerId = 1,
                Date = new DateTime(2020, 1, 1, 1, 1, 1, 1),
                OrderedItems = orderedItems,
                Total = -0.02
            };
            var fakeRepo = new FakeOrderRepository
            {
                Customer = customer,
                Products = productsInStock
            };

            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            });
            var mapper = mockMapper.CreateMapper();

            var sp = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();
            var logFactory = sp.GetService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<OrderController>();

            var fakeFacade = new FakeStaffProductFacade();

            var controller = new OrderController(logger, fakeRepo, mapper, fakeFacade);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.IsType<UnprocessableEntityResult>(result);
        }
    }
}
