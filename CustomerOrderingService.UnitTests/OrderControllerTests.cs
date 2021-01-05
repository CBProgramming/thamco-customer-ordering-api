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
using Review.Facade;
using Review.Facade.Models;

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
        private Mock<IReviewFacade> mockReviewFacade;
        private Mock<IStaffProductFacade> mockProductFacade;
        private FakeStaffProductFacade fakeProductFacade;
        private FakeInvoiceFacade fakeInvoiceFacade;
        private FakeReviewFacade fakeReviewFacade;
        private bool repoSucceeds = true;
        private bool invoiceFacadeSucceeds = true;
        private bool productFacadeSucceeds = true;
        private bool reviewFacadeSucceeds = true;
        private bool productExists = true;
        private bool customerExists = true;
        private bool customerActive = true;
        private bool ordersExist = true;
        private bool productsInStock = true;

        private void SetMockProductRepo()
        {
            mockRepo = new Mock<IOrderRepository>(MockBehavior.Strict);
            mockRepo.Setup(repo => repo.ProductExists(It.IsAny<int>())).ReturnsAsync(productExists && repoSucceeds).Verifiable();
            mockRepo.Setup(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>())).ReturnsAsync(productExists && repoSucceeds).Verifiable();
            mockRepo.Setup(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>())).ReturnsAsync(productsInStock && repoSucceeds).Verifiable();
            mockRepo.Setup(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>())).ReturnsAsync(repoSucceeds).Verifiable();
            mockRepo.Setup(repo => repo.EditProduct(It.IsAny<ProductRepoModel>())).ReturnsAsync(repoSucceeds).Verifiable();
            mockRepo.Setup(repo => repo.DeleteProduct(It.IsAny<int>())).ReturnsAsync(repoSucceeds).Verifiable();
            mockRepo.Setup(repo => repo.CustomerExists(It.IsAny<int>())).ReturnsAsync(repoSucceeds && customerExists).Verifiable();
            mockRepo.Setup(repo => repo.IsCustomerActive(It.IsAny<int>())).ReturnsAsync(repoSucceeds && customerActive).Verifiable();
            mockRepo.Setup(repo => repo.GetCustomerOrders(It.IsAny<int>())).ReturnsAsync(ordersExist?orderRepoModels:new List<OrderRepoModel>()).Verifiable();
            mockRepo.Setup(repo => repo.GetCustomerOrder(It.IsAny<int>())).ReturnsAsync(ordersExist?new OrderRepoModel():null).Verifiable();
            mockRepo.Setup(repo => repo.GetOrderItems(It.IsAny<int>())).ReturnsAsync(new List<OrderedItemRepoModel>()).Verifiable();
            mockRepo.Setup(repo => repo.GetCustomer(It.IsAny<int>())).ReturnsAsync(customerExists&&customerActive&&repoSucceeds?customerRepoModel:null).Verifiable();
            mockRepo.Setup(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>())).ReturnsAsync(repoSucceeds ? 1 : 0).Verifiable();
            mockRepo.Setup(repo => repo.ClearBasket(It.IsAny<int>())).ReturnsAsync(repoSucceeds).Verifiable();
        }

        private void SetMockInvoiceFacade()
        {
            mockInvoiceFacade = new Mock<IInvoiceFacade>(MockBehavior.Strict);
            mockInvoiceFacade.Setup(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>())).ReturnsAsync(invoiceFacadeSucceeds);
        }

        private void SetMockProductFacade()
        {
            mockProductFacade = new Mock<IStaffProductFacade>(MockBehavior.Strict);
            mockProductFacade.Setup(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>())).ReturnsAsync(productFacadeSucceeds);
        }

        private void SetupMockReviewFacade()
        {
            mockReviewFacade = new Mock<IReviewFacade>(MockBehavior.Strict);
            mockReviewFacade.Setup(facade => facade.NewPurchases(It.IsAny<PurchaseDto>())).ReturnsAsync(reviewFacadeSucceeds);
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
                Country = "Country",
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
                OrderedItems = orderedItems,
                Products = productRepoModels
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
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
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
                SetupMockReviewFacade();
                controller = new OrderController(logger, mockRepo.Object, mapper, mockProductFacade.Object, mockInvoiceFacade.Object, mockReviewFacade.Object);
            }
            else
            {
                SetupFakeRepo(customerRepoModel, orderRepoModels, orderedItemsRepoModels);
                fakeProductFacade = new FakeStaffProductFacade();
                fakeInvoiceFacade = new FakeInvoiceFacade();
                fakeReviewFacade = new FakeReviewFacade();
                controller = new OrderController(logger, fakeRepo, mapper, fakeProductFacade, fakeInvoiceFacade, fakeReviewFacade);
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
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
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
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
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
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
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
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
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
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(customerId), Times.Once);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task AsCustomer_GetOrder_ShouldOkObject()
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
            Assert.True(fakeRepo.Orders[orderRequested].OrderId == orderResult.OrderId);
            Assert.True(fakeRepo.Orders[orderRequested].OrderDate == orderResult.OrderDate);
            Assert.True(fakeRepo.Orders[orderRequested].Total == orderResult.Total);
            Assert.True(orderedItemsRepoModels.Count == orderResult.OrderedItems.Count);
            for (int i = 0; i < orderResult.OrderedItems.Count; i++)
            {
                Assert.True(orderedItemsRepoModels[i].OrderId == orderResult.OrderedItems[i].OrderId);
                Assert.True(orderedItemsRepoModels[i].Name == orderResult.OrderedItems[i].Name);
                Assert.True(orderedItemsRepoModels[i].ProductId == orderResult.OrderedItems[i].ProductId);
                Assert.True(orderedItemsRepoModels[i].Price == orderResult.OrderedItems[i].Price);
                Assert.True(orderedItemsRepoModels[i].Quantity == orderResult.OrderedItems[i].Quantity);
            }
        }

        [Fact]
        public async Task AsCustomer_GetOrder_CheckMocks()
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
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task AsCustomer_GetOrder_InvalidId_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            int orderRequested = 99;

            //Act
            var result = await controller.Get(1, orderRequested);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async Task AsCustomer_GetOrder_InvalidId_CheckMocks()
        {
            //Arrange
            ordersExist = false;
            DefaultSetup(true);
            var customerId = 1;
            int orderRequested = 99;

            //Act
            var result = await controller.Get(customerId, orderRequested);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(orderRequested), Times.Once);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task AsCustomer_GetOrder_InactiveId_ShoulForbid()
        {
            //Arrange
            DefaultSetup();
            customerRepoModel.Active = false;
            int orderRequested = 1;

            //Act
            var result = await controller.Get(1, orderRequested);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async Task AsCustomer_GetOrder_InactiveId_CheckMocks()
        {
            //Arrange
            customerActive = false;
            DefaultSetup(true);
            var customerId = 1;
            int orderRequested = 1;

            //Act
            var result = await controller.Get(customerId, orderRequested);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task AsCustomer_GetOrder_InvalidAuthId_ShoulForbid()
        {
            //Arrange
            DefaultSetup();
            customerRepoModel.CustomerAuthId = "wrongId";
            int orderRequested = 1;

            //Act
            var result = await controller.Get(1, orderRequested);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async Task AsCustomer_GetOrder_InvalidAuthId_CheckMocks()
        {
            //Arrange
           
            DefaultSetup(true);
            var customerId = 1;
            int orderRequested = 1;
            customerRepoModel.CustomerAuthId = "wrongId";

            //Act
            var result = await controller.Get(customerId, orderRequested);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task AsCustomer_GetOrder_NoOrderedItems_ShouldOkObject()
        {
            //Arrange
            DefaultSetup();
            int orderRequested = 1;
            fakeRepo.OrderedItems = new List<OrderedItemRepoModel>();

            //Act
            var result = await controller.Get(1, orderRequested);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var orderResult = objResult.Value as OrderDto;
            Assert.NotNull(orderResult);
            Assert.True(fakeRepo.Orders[orderRequested].OrderId == orderResult.OrderId);
            Assert.True(fakeRepo.Orders[orderRequested].OrderDate == orderResult.OrderDate);
            Assert.True(fakeRepo.Orders[orderRequested].Total == orderResult.Total);
            Assert.True(0 == orderResult.OrderedItems.Count);
        }

        [Fact]
        public async Task AsCustomer_GetOrder_NoOrderedItems_CheckMocks()
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
            var orderResult = objResult.Value as OrderDto;
            Assert.NotNull(orderResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(customerId), Times.Once);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task AsStaff_GetOrderHistory_ShouldOkObject()
        {
            //Arrange
            DefaultSetup(setupStaff: true);
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
        public async Task AsStaff_GetOrderHistory_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupStaff: true);
            var customerId = 1;

            //Act
            var result = await controller.Get(customerId, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(customerId), Times.Once);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task AsStaff_GetOrderHistory_InvalidCustomerId_ShouldNotFound()
        {
            //Arrange
            DefaultSetup(setupStaff: true);

            //Act
            var result = await controller.Get(2, null);

            //Assert
            Assert.NotNull(result);
            var notResult = result as NotFoundResult;
            Assert.NotNull(notResult);
        }

        [Fact]
        public async Task AsStaff_GetOrderHistory_InvalidCustomerId_CheckMocks()
        {
            //Arrange
            customerExists = false;
            DefaultSetup(withMocks: true, setupStaff: true);
            var customerId = 2;

            //Act
            var result = await controller.Get(customerId, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(customerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task AsStaff_GetOrderHistory_InactiveCustomerId_ShouldNotFound()
        {
            //Arrange
            DefaultSetup(setupStaff: true);
            customerRepoModel.Active = false;

            //Act
            var result = await controller.Get(1, null);

            //Assert
            Assert.NotNull(result);
            var notResult = result as NotFoundResult;
            Assert.NotNull(notResult);
        }

        [Fact]
        public async Task AsStaff_GetOrderHistory_InactiveCustomerId_CheckMocks()
        {
            //Arrange
            customerActive = false;
            DefaultSetup(withMocks: true, setupStaff: true);
            var customerId = 2;
            customerRepoModel.Active = false;

            //Act
            var result = await controller.Get(customerId, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(customerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task AsStaff_GetOrderHistory_NoOrders()
        {
            //Arrange
            DefaultSetup(setupStaff: true);
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
        public async Task AsStaff_GetOrderHistory_NoOrders_CheckMocks()
        {
            //Arrange
            ordersExist = false;
            DefaultSetup(withMocks: true, setupStaff: true);
            var customerId = 1;

            //Act
            var result = await controller.Get(customerId, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(customerId), Times.Once);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task AsStaff_GetOrder_ShouldOkObject()
        {
            //Arrange

            DefaultSetup(setupStaff: true);
            int orderRequested = 1;

            //Act
            var result = await controller.Get(1, orderRequested);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var orderResult = objResult.Value as OrderDto;
            Assert.NotNull(orderResult);
            Assert.True(fakeRepo.Orders[orderRequested].OrderId == orderResult.OrderId);
            Assert.True(fakeRepo.Orders[orderRequested].OrderDate == orderResult.OrderDate);
            Assert.True(fakeRepo.Orders[orderRequested].Total == orderResult.Total);
            Assert.True(orderedItemsRepoModels.Count == orderResult.OrderedItems.Count);
            for (int i = 0; i < orderResult.OrderedItems.Count; i++)
            {
                Assert.True(orderedItemsRepoModels[i].OrderId == orderResult.OrderedItems[i].OrderId);
                Assert.True(orderedItemsRepoModels[i].Name == orderResult.OrderedItems[i].Name);
                Assert.True(orderedItemsRepoModels[i].ProductId == orderResult.OrderedItems[i].ProductId);
                Assert.True(orderedItemsRepoModels[i].Price == orderResult.OrderedItems[i].Price);
                Assert.True(orderedItemsRepoModels[i].Quantity == orderResult.OrderedItems[i].Quantity);
            }
        }

        [Fact]
        public async Task AsStaff_GetOrder_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupStaff: true);
            var customerId = 1;
            int orderRequested = 1;

            //Act
            var result = await controller.Get(customerId, orderRequested);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(customerId), Times.Once);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task AsStaff_GetOrder_InvalidId_ShouldNotFound()
        {
            //Arrange
            DefaultSetup(setupStaff: true);
            int orderRequested = 99;

            //Act
            var result = await controller.Get(1, orderRequested);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async Task AsStaff_GetOrder_InvalidId_CheckMocks()
        {
            //Arrange
            ordersExist = false;
            DefaultSetup(withMocks: true, setupStaff: true);
            var customerId = 1;
            int orderRequested = 99;

            //Act
            var result = await controller.Get(customerId, orderRequested);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(orderRequested), Times.Once);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task AsStaff_GetOrder_InactiveId_ShoulForbid()
        {
            //Arrange
            DefaultSetup(setupStaff: true);
            customerRepoModel.Active = false;
            int orderRequested = 1;

            //Act
            var result = await controller.Get(1, orderRequested);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async Task AsStaff_GetOrder_InactiveId_CheckMocks()
        {
            //Arrange
            customerActive = false;
            DefaultSetup(withMocks: true, setupStaff: true);
            var customerId = 1;
            int orderRequested = 1;

            //Act
            var result = await controller.Get(customerId, orderRequested);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task AsStaff_GetOrder_NoOrderedItems_ShouldOkObject()
        {
            //Arrange
            DefaultSetup(setupStaff: true);
            int orderRequested = 1;
            fakeRepo.OrderedItems = new List<OrderedItemRepoModel>();

            //Act
            var result = await controller.Get(1, orderRequested);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var orderResult = objResult.Value as OrderDto;
            Assert.NotNull(orderResult);
            Assert.True(fakeRepo.Orders[orderRequested].OrderId == orderResult.OrderId);
            Assert.True(fakeRepo.Orders[orderRequested].OrderDate == orderResult.OrderDate);
            Assert.True(fakeRepo.Orders[orderRequested].Total == orderResult.Total);
            Assert.True(0 == orderResult.OrderedItems.Count);
        }

        [Fact]
        public async Task AsStaff_GetOrder_NoOrderedItems_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupStaff: true);
            var customerId = 1;
            int orderRequested = 1;

            //Act
            var result = await controller.Get(customerId, orderRequested);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var orderResult = objResult.Value as OrderDto;
            Assert.NotNull(orderResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(customerId), Times.Once);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_ShouldOk()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(finalisedOrder.OrderDate, fakeRepo.FinalisedOrder.OrderDate);
            Assert.Equal(finalisedOrder.CustomerId, fakeRepo.FinalisedOrder.CustomerId);
            Assert.Equal(finalisedOrder.Total, fakeRepo.FinalisedOrder.Total);
            Assert.Equal(0, fakeRepo.FinalisedOrder.OrderId);
            Assert.Equal(finalisedOrder.OrderedItems.Count, fakeRepo.FinalisedOrder.OrderedItems.Count);
            for (int i = 0; i < fakeRepo.FinalisedOrder.OrderedItems.Count; i++)
            {
                Assert.Equal(finalisedOrder.OrderedItems[i].OrderId, fakeRepo.FinalisedOrder.OrderedItems[i].OrderId);
                Assert.Equal(finalisedOrder.OrderedItems[i].ProductId, fakeRepo.FinalisedOrder.OrderedItems[i].ProductId);
                Assert.Equal(finalisedOrder.OrderedItems[i].Quantity, fakeRepo.FinalisedOrder.OrderedItems[i].Quantity);
                Assert.Equal(finalisedOrder.OrderedItems[i].Price, fakeRepo.FinalisedOrder.OrderedItems[i].Price);
                Assert.Equal(finalisedOrder.OrderedItems[i].Name, fakeRepo.FinalisedOrder.OrderedItems[i].Name);
            }
        }

        [Fact]
        public async Task CreateOrder_ConfirmMock()
        {
            //Arrange
            DefaultSetup(true);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.ClearBasket(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Once);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Once);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_NegativeItemPrice_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup();
            finalisedOrder.OrderedItems[0].Price = -0.01;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_NegativeItemPrice_ConfirmMock()
        {
            //Arrange
            DefaultSetup(true);
            finalisedOrder.OrderedItems[0].Price = -0.01;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_ZeroItemPrice_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            finalisedOrder.OrderedItems[0].Price = -0.00;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(finalisedOrder.OrderDate, fakeRepo.FinalisedOrder.OrderDate);
            Assert.Equal(finalisedOrder.CustomerId, fakeRepo.FinalisedOrder.CustomerId);
            Assert.Equal(finalisedOrder.Total, fakeRepo.FinalisedOrder.Total);
            Assert.Equal(0, fakeRepo.FinalisedOrder.OrderId);
            Assert.Equal(finalisedOrder.OrderedItems.Count, fakeRepo.FinalisedOrder.OrderedItems.Count);
            for (int i = 0; i < fakeRepo.FinalisedOrder.OrderedItems.Count; i++)
            {
                Assert.Equal(finalisedOrder.OrderedItems[i].OrderId, fakeRepo.FinalisedOrder.OrderedItems[i].OrderId);
                Assert.Equal(finalisedOrder.OrderedItems[i].ProductId, fakeRepo.FinalisedOrder.OrderedItems[i].ProductId);
                Assert.Equal(finalisedOrder.OrderedItems[i].Quantity, fakeRepo.FinalisedOrder.OrderedItems[i].Quantity);
                Assert.Equal(finalisedOrder.OrderedItems[i].Price, fakeRepo.FinalisedOrder.OrderedItems[i].Price);
                Assert.Equal(finalisedOrder.OrderedItems[i].Name, fakeRepo.FinalisedOrder.OrderedItems[i].Name);
            }
        }

        [Fact]
        public async Task CreateOrder_ZeroItemPrice_ConfirmMock()
        {
            //Arrange
            DefaultSetup(true);
            finalisedOrder.OrderedItems[0].Price = -0.00;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.ClearBasket(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Once);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Once);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_NegativeTotalPrice_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup();
            finalisedOrder.Total = -0.01;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_NegativeTotalPrice_ConfirmMock()
        {
            //Arrange
            DefaultSetup(true);
            finalisedOrder.Total = -0.01;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_ZeroTotalPrice_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            finalisedOrder.Total = 0.00;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(finalisedOrder.OrderDate, fakeRepo.FinalisedOrder.OrderDate);
            Assert.Equal(finalisedOrder.CustomerId, fakeRepo.FinalisedOrder.CustomerId);
            Assert.Equal(finalisedOrder.Total, fakeRepo.FinalisedOrder.Total);
            Assert.Equal(0, fakeRepo.FinalisedOrder.OrderId);
            Assert.Equal(finalisedOrder.OrderedItems.Count, fakeRepo.FinalisedOrder.OrderedItems.Count);
            for (int i = 0; i < fakeRepo.FinalisedOrder.OrderedItems.Count; i++)
            {
                Assert.Equal(finalisedOrder.OrderedItems[i].OrderId, fakeRepo.FinalisedOrder.OrderedItems[i].OrderId);
                Assert.Equal(finalisedOrder.OrderedItems[i].ProductId, fakeRepo.FinalisedOrder.OrderedItems[i].ProductId);
                Assert.Equal(finalisedOrder.OrderedItems[i].Quantity, fakeRepo.FinalisedOrder.OrderedItems[i].Quantity);
                Assert.Equal(finalisedOrder.OrderedItems[i].Price, fakeRepo.FinalisedOrder.OrderedItems[i].Price);
                Assert.Equal(finalisedOrder.OrderedItems[i].Name, fakeRepo.FinalisedOrder.OrderedItems[i].Name);
            }
        }

        [Fact]
        public async Task CreateOrder_ZeroTotalPrice_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            finalisedOrder.Total = 0.00;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.ClearBasket(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Once);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Once);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_NegativeItemQuantity_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup();
            finalisedOrder.OrderedItems[0].Quantity = -1;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_NegativeItemQuantity_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            finalisedOrder.OrderedItems[0].Quantity = -1;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_ZeroItemQuantity_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup();
            finalisedOrder.OrderedItems[0].Quantity = 0;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_ZeroItemQuantity_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            finalisedOrder.OrderedItems[0].Quantity = 0;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_InvalidCustomerId_ShouldNotFound()
        {
            DefaultSetup();
            finalisedOrder.CustomerId = 2;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_InvalidCustomerId_CheckMocks()
        {
            customerExists = false;
            DefaultSetup(true);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_InvalidAuthId_ShouldForbid()
        {
            DefaultSetup();
            fakeRepo.Customer.CustomerAuthId = "WrongId";

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_InvalidAuthId_CheckMocks()
        {
            DefaultSetup(true);
            customerRepoModel.CustomerAuthId = "WrongId";

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_InvalidProductId_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            finalisedOrder.OrderedItems[0].ProductId = 99;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_InvalidProductId_CheckMocks()
        {
            productExists = false;
            DefaultSetup(true);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_CustomerCantPurchase_ShouldForbid()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.CanPurchase = false;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_CustomerCantPurchase_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            customerRepoModel.CanPurchase = false;
            
            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_CustomerNotActive_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.Active = false;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_CustomerNotActive_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            customerRepoModel.Active = false;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_OutOfStock_ShouldConflict()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Products[0].Quantity = 0;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ConflictResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_OutOfStock_CheckMocks()
        {
            //Arrange
            productsInStock = false;
            DefaultSetup(true);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ConflictResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(finalisedOrder.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_NotEnoughStock_ShouldConflict()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Products[0].Quantity = 1;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ConflictResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }


        [Fact]
        public async Task CreateOrder_NotEnoughStock_CheckMocks()
        {
            //Arrange
            productsInStock = false;
            DefaultSetup(true);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ConflictResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(finalisedOrder.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_NegativeQuantity_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup();
            finalisedOrder.OrderedItems[0].Quantity = -1;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_NegativeQuantity_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            finalisedOrder.OrderedItems[0].Quantity = -1;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Never);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);

        }

        [Fact]
        public async Task CreateOrder_FutureDate_ShouldOkWithTodaysDate()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }
            //Arrange
            DefaultSetup();
            finalisedOrder.OrderDate = new DateTime(2099, 1, 1, 1, 1, 1, 1);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.Equal(finalisedOrder.OrderDate.Year, DateTime.Now.Year);
            Assert.Equal(finalisedOrder.OrderDate.Month, DateTime.Now.Month);
            Assert.Equal(finalisedOrder.OrderDate.Day, DateTime.Now.Day);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(finalisedOrder.OrderDate, fakeRepo.FinalisedOrder.OrderDate);
            Assert.Equal(finalisedOrder.CustomerId, fakeRepo.FinalisedOrder.CustomerId);
            Assert.Equal(finalisedOrder.Total, fakeRepo.FinalisedOrder.Total);
            Assert.Equal(0, fakeRepo.FinalisedOrder.OrderId);
            Assert.Equal(finalisedOrder.OrderedItems.Count, fakeRepo.FinalisedOrder.OrderedItems.Count);
            for (int i = 0; i < fakeRepo.FinalisedOrder.OrderedItems.Count; i++)
            {
                Assert.Equal(finalisedOrder.OrderedItems[i].OrderId, fakeRepo.FinalisedOrder.OrderedItems[i].OrderId);
                Assert.Equal(finalisedOrder.OrderedItems[i].ProductId, fakeRepo.FinalisedOrder.OrderedItems[i].ProductId);
                Assert.Equal(finalisedOrder.OrderedItems[i].Quantity, fakeRepo.FinalisedOrder.OrderedItems[i].Quantity);
                Assert.Equal(finalisedOrder.OrderedItems[i].Price, fakeRepo.FinalisedOrder.OrderedItems[i].Price);
                Assert.Equal(finalisedOrder.OrderedItems[i].Name, fakeRepo.FinalisedOrder.OrderedItems[i].Name);
            }
        }

        [Fact]
        public async Task CreateOrder_FutureDate_CheckMocks()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }
            //Arrange
            DefaultSetup(true);
            finalisedOrder.OrderDate = new DateTime(2099, 1, 1, 1, 1, 1, 1);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.Equal(finalisedOrder.OrderDate.Year, DateTime.Now.Year);
            Assert.Equal(finalisedOrder.OrderDate.Month, DateTime.Now.Month);
            Assert.Equal(finalisedOrder.OrderDate.Day, DateTime.Now.Day);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.ClearBasket(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Once);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Once);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Once);
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
            DefaultSetup();
            finalisedOrder.OrderDate = DateTime.Now.Subtract(TimeSpan.FromDays(7));
            

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.Equal(finalisedOrder.OrderDate.Year, DateTime.Now.Year);
            Assert.Equal(finalisedOrder.OrderDate.Month, DateTime.Now.Month);
            Assert.Equal(finalisedOrder.OrderDate.Day, DateTime.Now.Day);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(finalisedOrder.OrderDate, fakeRepo.FinalisedOrder.OrderDate);
            Assert.Equal(finalisedOrder.CustomerId, fakeRepo.FinalisedOrder.CustomerId);
            Assert.Equal(finalisedOrder.Total, fakeRepo.FinalisedOrder.Total);
            Assert.Equal(0, fakeRepo.FinalisedOrder.OrderId);
            Assert.Equal(finalisedOrder.OrderedItems.Count, fakeRepo.FinalisedOrder.OrderedItems.Count);
            for (int i = 0; i < fakeRepo.FinalisedOrder.OrderedItems.Count; i++)
            {
                Assert.Equal(finalisedOrder.OrderedItems[i].OrderId, fakeRepo.FinalisedOrder.OrderedItems[i].OrderId);
                Assert.Equal(finalisedOrder.OrderedItems[i].ProductId, fakeRepo.FinalisedOrder.OrderedItems[i].ProductId);
                Assert.Equal(finalisedOrder.OrderedItems[i].Quantity, fakeRepo.FinalisedOrder.OrderedItems[i].Quantity);
                Assert.Equal(finalisedOrder.OrderedItems[i].Price, fakeRepo.FinalisedOrder.OrderedItems[i].Price);
                Assert.Equal(finalisedOrder.OrderedItems[i].Name, fakeRepo.FinalisedOrder.OrderedItems[i].Name);
            }
        }

        [Fact]
        public async Task CreateOrder_DateSevenDaysAgoExactly_CheckMocks()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }
            //Arrange
            DefaultSetup(true);
            finalisedOrder.OrderDate = DateTime.Now.Subtract(TimeSpan.FromDays(7));


            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.Equal(finalisedOrder.OrderDate.Year, DateTime.Now.Year);
            Assert.Equal(finalisedOrder.OrderDate.Month, DateTime.Now.Month);
            Assert.Equal(finalisedOrder.OrderDate.Day, DateTime.Now.Day);
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.ClearBasket(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Once);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Once);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Once);
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
            DefaultSetup();
            //set date two seconds before 7 day limit (any shorter a time and there's a risk of a correct failure)
            finalisedOrder.OrderDate = DateTime.Now.Subtract(TimeSpan.FromDays(7)).Add(TimeSpan.FromSeconds(2));
            int year = finalisedOrder.OrderDate.Year;
            int month = finalisedOrder.OrderDate.Month;
            int day = finalisedOrder.OrderDate.Day;
            int hour = finalisedOrder.OrderDate.Hour;
            int minute = finalisedOrder.OrderDate.Minute;
            int second = finalisedOrder.OrderDate.Second;
            int millisecond = finalisedOrder.OrderDate.Millisecond;
            
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
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(finalisedOrder.OrderDate, fakeRepo.FinalisedOrder.OrderDate);
            Assert.Equal(finalisedOrder.CustomerId, fakeRepo.FinalisedOrder.CustomerId);
            Assert.Equal(finalisedOrder.Total, fakeRepo.FinalisedOrder.Total);
            Assert.Equal(0, fakeRepo.FinalisedOrder.OrderId);
            Assert.Equal(finalisedOrder.OrderedItems.Count, fakeRepo.FinalisedOrder.OrderedItems.Count);
            for (int i = 0; i < fakeRepo.FinalisedOrder.OrderedItems.Count; i++)
            {
                Assert.Equal(finalisedOrder.OrderedItems[i].OrderId, fakeRepo.FinalisedOrder.OrderedItems[i].OrderId);
                Assert.Equal(finalisedOrder.OrderedItems[i].ProductId, fakeRepo.FinalisedOrder.OrderedItems[i].ProductId);
                Assert.Equal(finalisedOrder.OrderedItems[i].Quantity, fakeRepo.FinalisedOrder.OrderedItems[i].Quantity);
                Assert.Equal(finalisedOrder.OrderedItems[i].Price, fakeRepo.FinalisedOrder.OrderedItems[i].Price);
                Assert.Equal(finalisedOrder.OrderedItems[i].Name, fakeRepo.FinalisedOrder.OrderedItems[i].Name);
            }
        }

        [Fact]
        public async Task CreateOrder_AlmostSevenDaysAgo_CheckMocks()
        {
            //wait two seconds in case datetime day/month/year is about to change
            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second == 58)
            {
                System.Threading.Thread.Sleep(2000);
            }
            //Arrange
            DefaultSetup(true);
            //set date two seconds before 7 day limit (any shorter a time and there's a risk of a correct failure)
            finalisedOrder.OrderDate = DateTime.Now.Subtract(TimeSpan.FromDays(7)).Add(TimeSpan.FromSeconds(2));
            int year = finalisedOrder.OrderDate.Year;
            int month = finalisedOrder.OrderDate.Month;
            int day = finalisedOrder.OrderDate.Day;
            int hour = finalisedOrder.OrderDate.Hour;
            int minute = finalisedOrder.OrderDate.Minute;
            int second = finalisedOrder.OrderDate.Second;
            int millisecond = finalisedOrder.OrderDate.Millisecond;

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
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.ClearBasket(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Once);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Once);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_NoOrderedItems()
        {
            //Arrange
            DefaultSetup();
            finalisedOrder.OrderedItems = new List<OrderedItemDto>();

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_NoOrderedItems_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            finalisedOrder.OrderedItems = new List<OrderedItemDto>();

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_NullOrderedItems()
        {
            //Arrange
            DefaultSetup();
            finalisedOrder.OrderedItems = null;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_NullOrderedItems_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            finalisedOrder.OrderedItems = null;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_ZeroUnitPrice_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            finalisedOrder.OrderedItems[0].Price = 0;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(finalisedOrder.OrderDate, fakeRepo.FinalisedOrder.OrderDate);
            Assert.Equal(finalisedOrder.CustomerId, fakeRepo.FinalisedOrder.CustomerId);
            Assert.Equal(finalisedOrder.Total, fakeRepo.FinalisedOrder.Total);
            Assert.Equal(0, fakeRepo.FinalisedOrder.OrderId);
            Assert.Equal(finalisedOrder.OrderedItems.Count, fakeRepo.FinalisedOrder.OrderedItems.Count);
            for (int i = 0; i < fakeRepo.FinalisedOrder.OrderedItems.Count; i++)
            {
                Assert.Equal(finalisedOrder.OrderedItems[i].OrderId, fakeRepo.FinalisedOrder.OrderedItems[i].OrderId);
                Assert.Equal(finalisedOrder.OrderedItems[i].ProductId, fakeRepo.FinalisedOrder.OrderedItems[i].ProductId);
                Assert.Equal(finalisedOrder.OrderedItems[i].Quantity, fakeRepo.FinalisedOrder.OrderedItems[i].Quantity);
                Assert.Equal(finalisedOrder.OrderedItems[i].Price, fakeRepo.FinalisedOrder.OrderedItems[i].Price);
                Assert.Equal(finalisedOrder.OrderedItems[i].Name, fakeRepo.FinalisedOrder.OrderedItems[i].Name);
            }
        }

        [Fact]
        public async Task CreateOrder_ZeroUnitPrice_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            finalisedOrder.OrderedItems[0].Price = 0;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.ClearBasket(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Once);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Once);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_RepoFailure_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.CompletesOrders = false;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_RepoFailure_CheckMocks()
        {
            //Arrange
            repoSucceeds = false;
            DefaultSetup(true);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_ProductFacadeFailure_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            fakeProductFacade.CompletesStockReduction = false;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder != null);
        }

        [Fact]
        public async Task CreateOrder_ProductFacadeFailure_CheckMocks()
        {
            //Arrange
            productFacadeSucceeds = false;
            DefaultSetup(true);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.ClearBasket(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Once);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Once);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_InvoiceFacadeFailure_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            fakeInvoiceFacade.Succeeds = false;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);

            Assert.Equal(finalisedOrder.OrderDate, fakeRepo.FinalisedOrder.OrderDate);
            Assert.Equal(finalisedOrder.CustomerId, fakeRepo.FinalisedOrder.CustomerId);
            Assert.Equal(finalisedOrder.Total, fakeRepo.FinalisedOrder.Total);
            Assert.Equal(0, fakeRepo.FinalisedOrder.OrderId);
            Assert.Equal(finalisedOrder.OrderedItems.Count, fakeRepo.FinalisedOrder.OrderedItems.Count);
            for (int i = 0; i < fakeRepo.FinalisedOrder.OrderedItems.Count; i++)
            {
                Assert.Equal(finalisedOrder.OrderedItems[i].OrderId, fakeRepo.FinalisedOrder.OrderedItems[i].OrderId);
                Assert.Equal(finalisedOrder.OrderedItems[i].ProductId, fakeRepo.FinalisedOrder.OrderedItems[i].ProductId);
                Assert.Equal(finalisedOrder.OrderedItems[i].Quantity, fakeRepo.FinalisedOrder.OrderedItems[i].Quantity);
                Assert.Equal(finalisedOrder.OrderedItems[i].Price, fakeRepo.FinalisedOrder.OrderedItems[i].Price);
                Assert.Equal(finalisedOrder.OrderedItems[i].Name, fakeRepo.FinalisedOrder.OrderedItems[i].Name);
            }
        }

        [Fact]
        public async Task CreateOrder_InvoiceFacadeFailure_CheckMocks()
        {
            //Arrange
            invoiceFacadeSucceeds = false;
            DefaultSetup(true);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.ClearBasket(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Once);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Once);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_ReviewFacadeFailure_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            fakeReviewFacade.Succeeds = false;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);

            Assert.Equal(finalisedOrder.OrderDate, fakeRepo.FinalisedOrder.OrderDate);
            Assert.Equal(finalisedOrder.CustomerId, fakeRepo.FinalisedOrder.CustomerId);
            Assert.Equal(finalisedOrder.Total, fakeRepo.FinalisedOrder.Total);
            Assert.Equal(0, fakeRepo.FinalisedOrder.OrderId);
            Assert.Equal(finalisedOrder.OrderedItems.Count, fakeRepo.FinalisedOrder.OrderedItems.Count);
            for (int i = 0; i < fakeRepo.FinalisedOrder.OrderedItems.Count; i++)
            {
                Assert.Equal(finalisedOrder.OrderedItems[i].OrderId, fakeRepo.FinalisedOrder.OrderedItems[i].OrderId);
                Assert.Equal(finalisedOrder.OrderedItems[i].ProductId, fakeRepo.FinalisedOrder.OrderedItems[i].ProductId);
                Assert.Equal(finalisedOrder.OrderedItems[i].Quantity, fakeRepo.FinalisedOrder.OrderedItems[i].Quantity);
                Assert.Equal(finalisedOrder.OrderedItems[i].Price, fakeRepo.FinalisedOrder.OrderedItems[i].Price);
                Assert.Equal(finalisedOrder.OrderedItems[i].Name, fakeRepo.FinalisedOrder.OrderedItems[i].Name);
            }
        }

        [Fact]
        public async Task CreateOrder_ReviewFacadeFailure_CheckMocks()
        {
            //Arrange
            reviewFacadeSucceeds = false;
            DefaultSetup(true);

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.ClearBasket(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Once);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Once);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_BarelyValidAddressWithNulls_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.AddressTwo = null;
            fakeRepo.Customer.Town = null;
            fakeRepo.Customer.State = null;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(finalisedOrder.OrderDate, fakeRepo.FinalisedOrder.OrderDate);
            Assert.Equal(finalisedOrder.CustomerId, fakeRepo.FinalisedOrder.CustomerId);
            Assert.Equal(finalisedOrder.Total, fakeRepo.FinalisedOrder.Total);
            Assert.Equal(0, fakeRepo.FinalisedOrder.OrderId);
            Assert.Equal(finalisedOrder.OrderedItems.Count, fakeRepo.FinalisedOrder.OrderedItems.Count);
            for (int i = 0; i < fakeRepo.FinalisedOrder.OrderedItems.Count; i++)
            {
                Assert.Equal(finalisedOrder.OrderedItems[i].OrderId, fakeRepo.FinalisedOrder.OrderedItems[i].OrderId);
                Assert.Equal(finalisedOrder.OrderedItems[i].ProductId, fakeRepo.FinalisedOrder.OrderedItems[i].ProductId);
                Assert.Equal(finalisedOrder.OrderedItems[i].Quantity, fakeRepo.FinalisedOrder.OrderedItems[i].Quantity);
                Assert.Equal(finalisedOrder.OrderedItems[i].Price, fakeRepo.FinalisedOrder.OrderedItems[i].Price);
                Assert.Equal(finalisedOrder.OrderedItems[i].Name, fakeRepo.FinalisedOrder.OrderedItems[i].Name);
            }
        }

        [Fact]
        public async Task CreateOrder_BarelyValidAddressWithNulls_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            customerRepoModel.AddressTwo = null;
            customerRepoModel.Town = null;
            customerRepoModel.State = null;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.ClearBasket(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Once);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Once);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_AddressTwoNull_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.AddressTwo = null;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(finalisedOrder.OrderDate, fakeRepo.FinalisedOrder.OrderDate);
            Assert.Equal(finalisedOrder.CustomerId, fakeRepo.FinalisedOrder.CustomerId);
            Assert.Equal(finalisedOrder.Total, fakeRepo.FinalisedOrder.Total);
            Assert.Equal(0, fakeRepo.FinalisedOrder.OrderId);
            Assert.Equal(finalisedOrder.OrderedItems.Count, fakeRepo.FinalisedOrder.OrderedItems.Count);
            for (int i = 0; i < fakeRepo.FinalisedOrder.OrderedItems.Count; i++)
            {
                Assert.Equal(finalisedOrder.OrderedItems[i].OrderId, fakeRepo.FinalisedOrder.OrderedItems[i].OrderId);
                Assert.Equal(finalisedOrder.OrderedItems[i].ProductId, fakeRepo.FinalisedOrder.OrderedItems[i].ProductId);
                Assert.Equal(finalisedOrder.OrderedItems[i].Quantity, fakeRepo.FinalisedOrder.OrderedItems[i].Quantity);
                Assert.Equal(finalisedOrder.OrderedItems[i].Price, fakeRepo.FinalisedOrder.OrderedItems[i].Price);
                Assert.Equal(finalisedOrder.OrderedItems[i].Name, fakeRepo.FinalisedOrder.OrderedItems[i].Name);
            }
        }

        [Fact]
        public async Task CreateOrder_AddressTwoNull_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            customerRepoModel.AddressTwo = null;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.ClearBasket(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Once);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Once);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_TownNull_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.Town = null;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(finalisedOrder.OrderDate, fakeRepo.FinalisedOrder.OrderDate);
            Assert.Equal(finalisedOrder.CustomerId, fakeRepo.FinalisedOrder.CustomerId);
            Assert.Equal(finalisedOrder.Total, fakeRepo.FinalisedOrder.Total);
            Assert.Equal(0, fakeRepo.FinalisedOrder.OrderId);
            Assert.Equal(finalisedOrder.OrderedItems.Count, fakeRepo.FinalisedOrder.OrderedItems.Count);
            for (int i = 0; i < fakeRepo.FinalisedOrder.OrderedItems.Count; i++)
            {
                Assert.Equal(finalisedOrder.OrderedItems[i].OrderId, fakeRepo.FinalisedOrder.OrderedItems[i].OrderId);
                Assert.Equal(finalisedOrder.OrderedItems[i].ProductId, fakeRepo.FinalisedOrder.OrderedItems[i].ProductId);
                Assert.Equal(finalisedOrder.OrderedItems[i].Quantity, fakeRepo.FinalisedOrder.OrderedItems[i].Quantity);
                Assert.Equal(finalisedOrder.OrderedItems[i].Price, fakeRepo.FinalisedOrder.OrderedItems[i].Price);
                Assert.Equal(finalisedOrder.OrderedItems[i].Name, fakeRepo.FinalisedOrder.OrderedItems[i].Name);
            }
        }

        [Fact]
        public async Task CreateOrder_TownNull_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            customerRepoModel.Town = null;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.ClearBasket(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Once);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Once);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_StateNull_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.State = null;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(finalisedOrder.OrderDate, fakeRepo.FinalisedOrder.OrderDate);
            Assert.Equal(finalisedOrder.CustomerId, fakeRepo.FinalisedOrder.CustomerId);
            Assert.Equal(finalisedOrder.Total, fakeRepo.FinalisedOrder.Total);
            Assert.Equal(0, fakeRepo.FinalisedOrder.OrderId);
            Assert.Equal(finalisedOrder.OrderedItems.Count, fakeRepo.FinalisedOrder.OrderedItems.Count);
            for (int i = 0; i < fakeRepo.FinalisedOrder.OrderedItems.Count; i++)
            {
                Assert.Equal(finalisedOrder.OrderedItems[i].OrderId, fakeRepo.FinalisedOrder.OrderedItems[i].OrderId);
                Assert.Equal(finalisedOrder.OrderedItems[i].ProductId, fakeRepo.FinalisedOrder.OrderedItems[i].ProductId);
                Assert.Equal(finalisedOrder.OrderedItems[i].Quantity, fakeRepo.FinalisedOrder.OrderedItems[i].Quantity);
                Assert.Equal(finalisedOrder.OrderedItems[i].Price, fakeRepo.FinalisedOrder.OrderedItems[i].Price);
                Assert.Equal(finalisedOrder.OrderedItems[i].Name, fakeRepo.FinalisedOrder.OrderedItems[i].Name);
            }
        }

        [Fact]
        public async Task CreateOrder_StateNull_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            customerRepoModel.State = null;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.ClearBasket(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Once);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Once);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_BarelyValidAddressWithBlanks_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.AddressTwo = "";
            fakeRepo.Customer.Town = "";
            fakeRepo.Customer.State = "";

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(finalisedOrder.OrderDate, fakeRepo.FinalisedOrder.OrderDate);
            Assert.Equal(finalisedOrder.CustomerId, fakeRepo.FinalisedOrder.CustomerId);
            Assert.Equal(finalisedOrder.Total, fakeRepo.FinalisedOrder.Total);
            Assert.Equal(0, fakeRepo.FinalisedOrder.OrderId);
            Assert.Equal(finalisedOrder.OrderedItems.Count, fakeRepo.FinalisedOrder.OrderedItems.Count);
            for (int i = 0; i < fakeRepo.FinalisedOrder.OrderedItems.Count; i++)
            {
                Assert.Equal(finalisedOrder.OrderedItems[i].OrderId, fakeRepo.FinalisedOrder.OrderedItems[i].OrderId);
                Assert.Equal(finalisedOrder.OrderedItems[i].ProductId, fakeRepo.FinalisedOrder.OrderedItems[i].ProductId);
                Assert.Equal(finalisedOrder.OrderedItems[i].Quantity, fakeRepo.FinalisedOrder.OrderedItems[i].Quantity);
                Assert.Equal(finalisedOrder.OrderedItems[i].Price, fakeRepo.FinalisedOrder.OrderedItems[i].Price);
                Assert.Equal(finalisedOrder.OrderedItems[i].Name, fakeRepo.FinalisedOrder.OrderedItems[i].Name);
            }
        }

        [Fact]
        public async Task CreateOrder_BarelyValidAddressWithBlanks_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            customerRepoModel.AddressTwo = "";
            customerRepoModel.Town = "";
            customerRepoModel.State = "";

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.ClearBasket(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Once);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Once);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_AddressTwoBlank_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.AddressTwo = "";

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(finalisedOrder.OrderDate, fakeRepo.FinalisedOrder.OrderDate);
            Assert.Equal(finalisedOrder.CustomerId, fakeRepo.FinalisedOrder.CustomerId);
            Assert.Equal(finalisedOrder.Total, fakeRepo.FinalisedOrder.Total);
            Assert.Equal(0, fakeRepo.FinalisedOrder.OrderId);
            Assert.Equal(finalisedOrder.OrderedItems.Count, fakeRepo.FinalisedOrder.OrderedItems.Count);
            for (int i = 0; i < fakeRepo.FinalisedOrder.OrderedItems.Count; i++)
            {
                Assert.Equal(finalisedOrder.OrderedItems[i].OrderId, fakeRepo.FinalisedOrder.OrderedItems[i].OrderId);
                Assert.Equal(finalisedOrder.OrderedItems[i].ProductId, fakeRepo.FinalisedOrder.OrderedItems[i].ProductId);
                Assert.Equal(finalisedOrder.OrderedItems[i].Quantity, fakeRepo.FinalisedOrder.OrderedItems[i].Quantity);
                Assert.Equal(finalisedOrder.OrderedItems[i].Price, fakeRepo.FinalisedOrder.OrderedItems[i].Price);
                Assert.Equal(finalisedOrder.OrderedItems[i].Name, fakeRepo.FinalisedOrder.OrderedItems[i].Name);
            }
        }

        [Fact]
        public async Task CreateOrder_AddressTwoBlank_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            customerRepoModel.AddressTwo = "";

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.ClearBasket(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Once);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Once);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_TownBlank_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.Town = "";

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(finalisedOrder.OrderDate, fakeRepo.FinalisedOrder.OrderDate);
            Assert.Equal(finalisedOrder.CustomerId, fakeRepo.FinalisedOrder.CustomerId);
            Assert.Equal(finalisedOrder.Total, fakeRepo.FinalisedOrder.Total);
            Assert.Equal(0, fakeRepo.FinalisedOrder.OrderId);
            Assert.Equal(finalisedOrder.OrderedItems.Count, fakeRepo.FinalisedOrder.OrderedItems.Count);
            for (int i = 0; i < fakeRepo.FinalisedOrder.OrderedItems.Count; i++)
            {
                Assert.Equal(finalisedOrder.OrderedItems[i].OrderId, fakeRepo.FinalisedOrder.OrderedItems[i].OrderId);
                Assert.Equal(finalisedOrder.OrderedItems[i].ProductId, fakeRepo.FinalisedOrder.OrderedItems[i].ProductId);
                Assert.Equal(finalisedOrder.OrderedItems[i].Quantity, fakeRepo.FinalisedOrder.OrderedItems[i].Quantity);
                Assert.Equal(finalisedOrder.OrderedItems[i].Price, fakeRepo.FinalisedOrder.OrderedItems[i].Price);
                Assert.Equal(finalisedOrder.OrderedItems[i].Name, fakeRepo.FinalisedOrder.OrderedItems[i].Name);
            }
        }

        [Fact]
        public async Task CreateOrder_TownBlank_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            customerRepoModel.Town = "";

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Once);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Once);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Once);

        }

        [Fact]
        public async Task CreateOrder_StateBlank_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.State = "";

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(finalisedOrder.OrderDate, fakeRepo.FinalisedOrder.OrderDate);
            Assert.Equal(finalisedOrder.CustomerId, fakeRepo.FinalisedOrder.CustomerId);
            Assert.Equal(finalisedOrder.Total, fakeRepo.FinalisedOrder.Total);
            Assert.Equal(0, fakeRepo.FinalisedOrder.OrderId);
            Assert.Equal(finalisedOrder.OrderedItems.Count, fakeRepo.FinalisedOrder.OrderedItems.Count);
            for (int i = 0; i < fakeRepo.FinalisedOrder.OrderedItems.Count; i++)
            {
                Assert.Equal(finalisedOrder.OrderedItems[i].OrderId, fakeRepo.FinalisedOrder.OrderedItems[i].OrderId);
                Assert.Equal(finalisedOrder.OrderedItems[i].ProductId, fakeRepo.FinalisedOrder.OrderedItems[i].ProductId);
                Assert.Equal(finalisedOrder.OrderedItems[i].Quantity, fakeRepo.FinalisedOrder.OrderedItems[i].Quantity);
                Assert.Equal(finalisedOrder.OrderedItems[i].Price, fakeRepo.FinalisedOrder.OrderedItems[i].Price);
                Assert.Equal(finalisedOrder.OrderedItems[i].Name, fakeRepo.FinalisedOrder.OrderedItems[i].Name);
            }
        }

        [Fact]
        public async Task CreateOrder_StateBlank_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            customerRepoModel.State = "";

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.ClearBasket(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Once);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Once);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrder_AddressOneNull_ShouldForbid()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.AddressOne = null;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_AddressOneNull_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            customerRepoModel.AddressOne = null;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_AreaCodeNull_ShouldForbid()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.AreaCode = null;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_AreaCodeNull_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            customerRepoModel.AreaCode = null;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_CountryNull_ShouldForbid()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.Country = null;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_CountryNull_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            customerRepoModel.Country = null;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_TelephoneNumberNull_ShouldForbid()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.TelephoneNumber = null;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_TelephoneNumberNull_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            customerRepoModel.TelephoneNumber = null;

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_AddressOneBlank_ShouldForbid()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.AddressOne = "";

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_AddressOneBlank_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            customerRepoModel.AddressOne = "";

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_AreaCodeBlank_ShouldForbid()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.AreaCode = "";

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_AreaCodeBlank_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            customerRepoModel.AreaCode = "";

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_CountryBlank_ShouldForbid()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.Country = "";

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_CountryBlank_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            customerRepoModel.Country = "";

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_TelephoneNumberBlank_ShouldForbid()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.TelephoneNumber = "";

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.FinalisedOrder == null);
        }

        [Fact]
        public async Task CreateOrder_TelephoneNumberBlank_CheckMocks()
        {
            //Arrange
            DefaultSetup(true);
            customerRepoModel.TelephoneNumber = "";

            //Act
            var result = await controller.Create(finalisedOrder);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.GetCustomer(finalisedOrder.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrders(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomerOrder(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.GetOrderItems(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateOrder(It.IsAny<FinalisedOrderRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.ClearBasket(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsExist(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.ProductsInStock(It.IsAny<List<ProductRepoModel>>()), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
            mockInvoiceFacade.Verify(facade => facade.NewOrder(It.IsAny<OrderInvoiceDto>()), Times.Never);
            mockProductFacade.Verify(facade => facade.UpdateStock(It.IsAny<List<StockReductionDto>>()), Times.Never);
            mockReviewFacade.Verify(facade => facade.NewPurchases(It.IsAny<PurchaseDto>()), Times.Never);
        }
    }
}
