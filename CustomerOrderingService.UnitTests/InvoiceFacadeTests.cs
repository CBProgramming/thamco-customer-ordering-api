using HttpManager;
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
        private Mock<IHttpHandler> mockHttpHandler;
        private string invoiceUriValue = "/api/Invoice/";
        private string staffAuthServerUrlKeyValue = "StaffAuthServerUrl";
        private string invoiceApiKeyValue = "InvoiceAPI";
        private string invoiceScopeKeyValue = "InvoiceScope";
        Uri expectedUri = new Uri("http://test/api/Invoice/");

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

        private void SetupConfig(string invoiceUri = null, string staffAuthUrlKey = null, string? invoiceAPIKey = null,
            string? invoiceScope = null)
        {
            var myConfiguration = new Dictionary<string, string>
             {
                {"StaffAuthServerUrlKey", staffAuthUrlKey??staffAuthServerUrlKeyValue},
                {"InvoiceAPIKey", invoiceAPIKey??invoiceApiKeyValue},
                {"InvoiceScopeKey", invoiceScope??invoiceScopeKeyValue},
                {"InvoiceUri" , invoiceUri?? invoiceUriValue },
                {"ClientId", "ClientId"},
                {"ClientSecret", "ClientSecret"}
            };
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

        private void SetupHttpHandlerMock()
        {
            mockHttpHandler = new Mock<IHttpHandler>(MockBehavior.Strict);
            mockHttpHandler.Setup(f => f.GetClient(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(client)).Verifiable();
        }

        private void DefaultSetup(HttpStatusCode statusCode)
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
            SetupHttpHandlerMock();
            facade = new InvoiceFacade(config, mockHttpHandler.Object);
            SetupConfig();
        }

        [Fact]
        public async Task UpdateStock_ShouldReturnTrue()
        {
            //Arrange
            DefaultSetup(HttpStatusCode.OK);

            //Act
            var result = await facade.NewOrder(order);

            //Assert
            Assert.True(true == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Once(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post && req.RequestUri == expectedUri), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient(staffAuthServerUrlKeyValue, invoiceApiKeyValue,
                invoiceScopeKeyValue), Times.Once);
        }

        [Fact]
        public async Task UpdateStock_NotFound_ShouldReturnFalse()
        {
            //Arrange
            DefaultSetup(HttpStatusCode.NotFound);

            //Act
            var result = await facade.NewOrder(order);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Once(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient(staffAuthServerUrlKeyValue, invoiceApiKeyValue,
                invoiceScopeKeyValue), Times.Once);
        }

        [Fact]
        public async Task UpdateStock_Null_ShouldReturnFalse()
        {
            //Arrange
            DefaultSetup(HttpStatusCode.OK);

            //Act
            var result = await facade.NewOrder(null);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStock_EmptyList_ShouldReturnFalse()
        {
            //Arrange
            DefaultSetup(HttpStatusCode.OK);
            order.OrderedItems = new List<InvoiceItemDto>();

            //Act
            var result = await facade.NewOrder(order);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStock_UriNull_ShouldReturnFalse()
        {
            //Arrange
            invoiceUriValue = null;
            DefaultSetup(HttpStatusCode.OK);

            //Act
            var result = await facade.NewOrder(order);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStock_UriEmpty_ShouldReturnFalse()
        {
            //Arrange
            invoiceUriValue = "";
            DefaultSetup(HttpStatusCode.OK);

            //Act
            var result = await facade.NewOrder(order);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStock_AuthKeyNull_ShouldReturnFalse()
        {
            //Arrange
            staffAuthServerUrlKeyValue = null;
            DefaultSetup(HttpStatusCode.OK);

            //Act
            var result = await facade.NewOrder(order);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStock_AuthKeyEmpty_ShouldReturnFalse()
        {
            //Arrange
            staffAuthServerUrlKeyValue = "";
            DefaultSetup(HttpStatusCode.OK);

            //Act
            var result = await facade.NewOrder(order);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStock_ApiKeyNull_ShouldReturnFalse()
        {
            //Arrange
            invoiceApiKeyValue = null;
            DefaultSetup(HttpStatusCode.OK);

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

        [Fact]
        public async Task UpdateStock_ApiKeyEmpty_ShouldReturnFalse()
        {
            //Arrange
            invoiceApiKeyValue = "";
            DefaultSetup(HttpStatusCode.OK);

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

        [Fact]
        public async Task UpdateStock_ScopeKeyNull_ShouldReturnFalse()
        {
            //Arrange
            invoiceScopeKeyValue = null;
            DefaultSetup(HttpStatusCode.OK);

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

        [Fact]
        public async Task UpdateStock_ScopeKeyEmpty_ShouldReturnFalse()
        {
            //Arrange
            invoiceScopeKeyValue = "";
            DefaultSetup(HttpStatusCode.OK);

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
