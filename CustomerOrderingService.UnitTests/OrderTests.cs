using AutoMapper;
using CustomerOrderingService.Controllers;
using CustomerOrderingService.Models;
using CustomerOrderingService.UnitTests.Fakes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Order.Repository.Models;
using StaffProduct.Facade;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CustomerOrderingService.UnitTests
{
    public class OrderTests
    {
        [Fact]
        public async Task GetOrderHistory()
        {
            //Arrange
            var customer = new CustomerEFModel
            {
                Id = 1,
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
            var fakeRepo = new FakeOrderRepo
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
            var facadeMock = new FakeStaffProductFacade();

            var controller = new OrderController(logger, fakeRepo, mapper, facadeMock);

            //Act
            var result = await controller.Get(1);

            //Assert
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var historyResult = objResult.Value as OrderHistoryDto;
            Assert.NotNull(historyResult);
            var customerResult = historyResult.Customer;
            Assert.Equal(customerResult.Name, customer.Name);
            Assert.Equal(customerResult.Id, customer.Id);
            var ordersResult = historyResult.Orders as List<OrderDto>;
            Assert.Equal(orders.Count, ordersResult.Count);
            for (int i = 0; i < orders.Count; i++)
            {
                Assert.Equal(orders[i].Id, ordersResult[i].Id);
                Assert.Equal(orders[i].OrderDate, ordersResult[i].OrderDate);
                Assert.Equal(orders[i].Total, ordersResult[i].Total);
                Assert.Equal(orderedItems.Count, ordersResult[i].Products.Count);
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
    }
}
