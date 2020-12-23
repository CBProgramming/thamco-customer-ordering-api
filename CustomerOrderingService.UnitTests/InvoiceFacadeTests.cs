using Invoicing.Facade;
using Invoicing.Facade.Models;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CustomerOrderingService.UnitTests
{
    public class InvoiceFacadeTests
    {
        public HttpClient client;
        public Mock<IHttpClientFactory> mockFactory;
        public Mock<HttpClient> mockClient;
        public Mock<HttpMessageHandler> mockHandler;
        public IInvoiceFacade facade;
        private IConfiguration config;
        private OrderInvoiceDto order;

        private void SetupOrder()
        {
            order = new OrderInvoiceDto
            {
                CustomerId = 1,
                OrderId = 1,
                OrderDate = new DateTime(),
                Total = 9.99,
                OrderedItems = new List<InvoiceItemDto>
                {
                    new InvoiceItemDto{ OrderId = 1, ProductId = 1, Quantity = 1, Price = 1, Name = "Product Name"},
                    new InvoiceItemDto{ OrderId = 2, ProductId = 2, Quantity = 2, Price = 2, Name = "Another Product Name"}
                }
            };
        }

        private void SetupConfig()
        {
            var myConfiguration = new Dictionary<string, string>
                {{"ConnectionStrings:ClientId", "clientId"},
                {"ConnectionStrings:ClientSecret", "clientSecret"},
                {"ConnectionStrings:CustomerAuthServerUrl", "https://fakeurl.com"},
                {"ConnectionStrings:StaffAuthServerUrl", "https://fakeurl.com"},
                {"ConnectionStrings:CustomerAccountUrl", "https://fakeurl.com"},
                {"ConnectionStrings:InvoiceUrl", "https://fakeurl.com"},
                {"ConnectionStrings:InvoiceUri", "fake/Uri"},
                {"ConnectionStrings:StaffProductUrl", "https://fakeurl.com"},
                {"ConnectionStrings:StaffProductUri", "/fake/Uri"},
                {"ConnectionStrings:ReviewUrl", "https://fakeurl.com"},
                {"ConnectionStrings:ReviewProductUri", "fake/Uri"}};

            config = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        private void SetMockMessageHandler(HttpResponseMessage expected)
        {
            mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expected)
                .Verifiable();
        }

        private void SetupRealHttpClient(HttpResponseMessage expected)
        {
            client = new HttpClient(mockHandler.Object);
            client.BaseAddress = new Uri("http://test");

        }

        private void SetupHttpFactoryMock(HttpClient client)
        {
            mockFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            mockFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client).Verifiable();
        }

        private void DefaultSetupRealHttpClient(HttpStatusCode statusCode)
        {
            SetupOrder();
            var expectedResult = new HttpResponseMessage
            {
                StatusCode = statusCode
            };
            SetMockMessageHandler(expectedResult);
            SetupRealHttpClient(expectedResult);
            SetupHttpFactoryMock(client);
            SetupConfig();
            facade = new InvoiceFacade(mockFactory.Object, config);
            SetupConfig();
        }

        [Fact]
        public async Task UpdateStock_ShouldReturnTrue()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/fake/Uri");

            //Act
            var result = await facade.NewOrder(order);

            //Assert
            Assert.True(true == result);
            mockHandler.Protected().Verify("SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post
                    && req.RequestUri == expectedUri),
                ItExpr.IsAny<CancellationToken>());
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateStock_NotFound_ShouldReturnFalse()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.NotFound);
            var expectedUri = new Uri("http://test/fake/Uri");

            //Act
            var result = await facade.NewOrder(order);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post
                    && req.RequestUri == expectedUri),
                ItExpr.IsAny<CancellationToken>());
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateStock_Null_ShouldReturnFalse()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/fake/Uri");

            //Act
            var result = await facade.NewOrder(null);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync",
                Times.Never(),
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post
                    && req.RequestUri == expectedUri),
                ItExpr.IsAny<CancellationToken>());
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStock_EmptyList_ShouldReturnFalse()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/fake/Uri");
            order.OrderedItems = new List<InvoiceItemDto>();

            //Act
            var result = await facade.NewOrder(order);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync",
                Times.Never(),
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post
                    && req.RequestUri == expectedUri),
                ItExpr.IsAny<CancellationToken>());
            mockFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Never);
        }
    }
}
