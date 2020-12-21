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
using Invoicing.Facade.Models;
using StaffProduct.Facade.Models;
using System.Security.Claims;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Http;

namespace CustomerOrderingService.UnitTests
{
    public class OrderControllerTests
    {
        private List<OrderRepoModel> orderRepoModels;
        private CustomerRepoModel customerRepoModel;
        private List<OrderedItemRepoModel> orderedItemsRepoModels;
        private IMapper mapper;
        private ILogger<OrderController> logger;
        private List<OrderedItemDto> orderedItemDtos;
        private List<ProductRepoModel> productRepoModels;
        private FinalisedOrderDto finalisedOrder;
        private FakeOrderRepository fakeRepo;
        private OrderController controller;
        private Mock<IOrderRepository> mockRepo;
        private Mock<IInvoiceFacade> mockInvoiceFacade;
        private Mock<IStaffProductFacade> mockProductFacade;
        private bool repoSucceeds = true;
        private bool invoiceFacadeSucceeds = true;
        private bool productFacadeSucceeds = true;
        private bool productExists = true;
        private bool customerExists = true;
        private bool customerActive = true;
        private bool ordersExist = true;

        private void SetMockProductRepo()
        {
            mockRepo = new Mock<IOrderRepository>(MockBehavior.Strict);
            mockRepo.Setup(repo => repo.ProductExists(It.IsAny<int>())).ReturnsAsync(productExists && repoSucceeds).Verifiable();
            mockRepo.Setup(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>())).ReturnsAsync(repoSucceeds).Verifiable();
            mockRepo.Setup(repo => repo.EditProduct(It.IsAny<ProductRepoModel>())).ReturnsAsync(repoSucceeds).Verifiable();
            mockRepo.Setup(repo => repo.DeleteProduct(It.IsAny<int>())).ReturnsAsync(repoSucceeds).Verifiable();
            mockRepo.Setup(repo => repo.CustomerExists(It.IsAny<int>())).ReturnsAsync(repoSucceeds && customerExists).Verifiable();
            mockRepo.Setup(repo => repo.IsCustomerActive(It.IsAny<int>())).ReturnsAsync(repoSucceeds && customerActive).Verifiable();
            mockRepo.Setup(repo => repo.GetCustomerOrders(It.IsAny<int>())).ReturnsAsync(ordersExist?orderRepoModels:new List<OrderRepoModel>()).Verifiable();
            mockRepo.Setup(repo => repo.GetCustomerOrder(It.IsAny<int>())).ReturnsAsync(ordersExist?new OrderRepoModel():null).Verifiable();
            mockRepo.Setup(repo => repo.GetOrderItems(It.IsAny<int>())).ReturnsAsync(new List<OrderedItemRepoModel>()).Verifiable();
            mockRepo.Setup(repo => repo.GetCustomer(It.IsAny<int>())).ReturnsAsync(customerExists?customerRepoModel:null).Verifiable();
        }

        private void SetMockInvoiceFacade(bool succeeds = true)
        {
            mockInvoiceFacade = new Mock<IInvoiceFacade>(MockBehavior.Strict);
            mockInvoiceFacade.Setup(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>())).ReturnsAsync(invoiceFacadeSucceeds);
        }

        private void SetMockProductFacade(bool succeeds = true)
        {
            mockProductFacade = new Mock<IStaffProductFacade>(MockBehavior.Strict);
            mockProductFacade.Setup(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>())).ReturnsAsync(productFacadeSucceeds);
        }

        private void SetupStandardOrderEFModels()
        {
            orderRepoModels =  new List<OrderRepoModel>()
            {
                new OrderRepoModel {OrderId = 1, OrderDate = new DateTime(2020,11,01), Total = 10.99 },
                new OrderRepoModel {OrderId = 2, OrderDate = new DateTime(2020,11,02), Total = 20.99 }
            };
        }

        private void SetupStandardCustomer()
        {
            customerRepoModel = new CustomerRepoModel
            {
                CustomerId = 1,
                CustomerAuthId = "fakeauthid",
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

        private void SetupStandardOrderedItemEFModels()
        {
            orderedItemsRepoModels = new List<OrderedItemRepoModel>()
            {
                new OrderedItemRepoModel{OrderId = 1, ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = 2},
                new OrderedItemRepoModel{OrderId = 1, ProductId = 1, Name = "Product 1", Price = 2.99, Quantity = 3},
                new OrderedItemRepoModel{OrderId = 1, ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
        }

        private void SetupMapper()
        {
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            }).CreateMapper();
        }

        private void SetupLogger()
        {
            logger = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider()
                .GetService<ILoggerFactory>()
                .CreateLogger<OrderController>();
        }

        private void SetupStandardOrderedItemDtos()
        {
            orderedItemDtos = new List<OrderedItemDto>()
            {
                new OrderedItemDto{ ProductId = 1, Name = "Product 1", Price = 1.99, Quantity = 2},
                new OrderedItemDto{ ProductId = 2, Name = "Product 2", Price = 3.99, Quantity = 5}
            };
        }

        private void SetupStandardProductsInStock()
        {
            productRepoModels = new List<ProductRepoModel>()
            {
                new ProductRepoModel{ ProductId = 1, Name = "Fake", Quantity = 10},
                new ProductRepoModel{ ProductId = 2, Name = "Fake", Quantity = 10},
                new ProductRepoModel{ ProductId = 3, Name = "Fake", Quantity = 10}
            };
        }

        private void SetupStandardFinalisedOrderDto()
        {
            finalisedOrder = new FinalisedOrderDto
            {
                CustomerId = 1,
                OrderDate = new DateTime(2020, 1, 1, 1, 1, 1, 1),
                OrderedItems = orderedItemDtos,
                Total = 5.98
            };
        }

        private void SetupFakeRepo(CustomerRepoModel customer, List<OrderRepoModel> orders, List<OrderedItemRepoModel> orderedItems)
        {
            fakeRepo = new FakeOrderRepository
            {
                Customer = customer,
                Orders = orders,
                OrderedItems = orderedItems
            };
        }

        private void SetupUser(bool setupStaff)
        {
            string app = setupStaff? "staff_web_app" : "customer_web_app";
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "name"),
                                        new Claim(ClaimTypes.Name, "name"),
                                        new Claim(OpenIdConnectConstants.Claims.Subject, "fakeauthid" ),
                                        new Claim("client_id", app),
                                        new Claim("role",setupStaff?"ManageCustomerAccounts":"Customer")
                                   }, "TestAuth"));
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        }

        private void ResetVars()
        {
            repoSucceeds = true;
            invoiceFacadeSucceeds = true;
            productFacadeSucceeds = true;
            productExists = true;
            customerExists = true;
            customerActive = true;
    }

        private void DefaultSetup(bool withMocks = false, bool setupStaff = false)
        {
            //Arrange
            SetupStandardOrderedItemDtos();
            SetupStandardProductsInStock();
            SetupStandardFinalisedOrderDto();
            SetupStandardCustomer();
            SetupStandardOrderEFModels();
            SetupStandardOrderedItemEFModels();
            SetupMapper();
            SetupLogger();
            if (withMocks)
            {
                SetMockProductRepo();
                SetMockInvoiceFacade();
                SetMockProductFacade();
                controller = new OrderController(logger, mockRepo.Object, mapper, mockProductFacade.Object, mockInvoiceFacade.Object);
            }
            else
            {
                SetupFakeRepo(customerRepoModel, orderRepoModels, orderedItemsRepoModels);
                var fakeProductFacade = new FakeStaffProductFacade();
                var fakeInvoiceFacade = new FakeInvoiceFacade();
                controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade);
            }
            SetupUser(setupStaff);
            ResetVars();
        }

        [Fact]
        public async Task AsCustomer_GetOrderHistory_ShouldOkObject()
        {
            //Arrange
            DefaultSetup();
            var customerId = 1;

            //Act
            var result = await controller.Get(customerId, null);

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
        public async Task AsCustomer_GetOrderHistory_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            var customerId = 1;

            //Act
            var result = await controller.Get(customerId, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(customerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerId), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(customerId), Times.Once);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
        }

        [Fact]
        public async Task AsCustomer_GetOrderHistory_InvalidCustomerId_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await controller.Get(2, null);

            //Assert
            Assert.NotNull(result);
            var notResult = result as NotFoundResult;
            Assert.NotNull(notResult);
        }

        [Fact]
        public async Task AsCustomer_GetOrderHistory_InvalidCustomerId_CheckMocks()
        {
            //Arrange
            customerExists = false;
            DefaultSetup(true);
            var customerId = 2;
            
            //Act
            var result = await controller.Get(customerId, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(customerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerId), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(customerId), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
        }

        [Fact]
        public async Task AsCustomer_GetOrderHistory_InactiveCustomerId_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            customerRepoModel.Active = false;

            //Act
            var result = await controller.Get(1, null);

            //Assert
            Assert.NotNull(result);
            var notResult = result as NotFoundResult;
            Assert.NotNull(notResult);
        }

        [Fact]
        public async Task AsCustomer_GetOrderHistory_InactiveCustomerId_CheckMocks()
        {
            //Arrange
            customerActive = false;
            DefaultSetup(true);
            var customerId = 2;
            customerRepoModel.Active = false;

            //Act
            var result = await controller.Get(customerId, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(customerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerId), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(customerId), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
        }

        [Fact]
        public async Task AsCustomer_GetOrderHistory_WrongAuthId_ShouldForbid()
        {
            //Arrange
            DefaultSetup();
            customerRepoModel.CustomerAuthId = "wrongId";
            var customerId = 1;

            //Act
            var result = await controller.Get(customerId, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async Task AsCustomer_GetOrderHistory_WrongAuthId_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            customerRepoModel.CustomerAuthId = "wrongId";
            var customerId = 1;

            //Act
            var result = await controller.Get(customerId, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(customerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerId), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(customerId), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
        }

        [Fact]
        public async Task AsCustomer_GetOrderHistory_NoOrders()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Orders = new List<OrderRepoModel>();

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
        public async Task AsCustomer_GetOrderHistory_NoOrders_CheckMocks()
        {
            //Arrange
            ordersExist = false;
            DefaultSetup(true);
            var customerId = 1;

            //Act
            var result = await controller.Get(customerId, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(customerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerId), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(customerId), Times.Once);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
        }

        [Fact]
        public async Task GetOrder_ShouldOkObject()
        {
            //Arrange

            DefaultSetup();
            int orderRequested = 1;

            //Act
            var result = await controller.Get(1, orderRequested);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var orderResult = objResult.Value as OrderDto;
            Assert.NotNull(orderResult);
            Assert.True(orderRepoModels[orderRequested].OrderId == orderResult.OrderId);
            Assert.True(orderRepoModels[orderRequested].OrderDate == orderResult.OrderDate);
            Assert.True(orderRepoModels[orderRequested].Total == orderResult.Total);
            Assert.True(orderedItemsRepoModels.Count == orderResult.Products.Count);
            for (int i = 0; i < orderResult.Products.Count; i++)
            {
                Assert.True(orderedItemsRepoModels[i].OrderId == orderResult.Products[i].OrderId);
                Assert.True(orderedItemsRepoModels[i].Name == orderResult.Products[i].Name);
                Assert.True(orderedItemsRepoModels[i].ProductId == orderResult.Products[i].ProductId);
                Assert.True(orderedItemsRepoModels[i].Price == orderResult.Products[i].Price);
                Assert.True(orderedItemsRepoModels[i].Quantity == orderResult.Products[i].Quantity);
            }
        }

        [Fact]
        public async Task GetOrder_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            var customerId = 1;
            int orderRequested = 1;

            //Act
            var result = await controller.Get(customerId, orderRequested);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(customerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerId), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(customerId), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(orderRequested), Times.Once);
            mockRepo.Verify(repo => repo.GetOrderItems(orderRequested), Times.Once);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
        }
    }
}
