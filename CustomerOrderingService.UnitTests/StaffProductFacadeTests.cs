using HttpManager;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using StaffProduct.Facade;
using StaffProduct.Facade.Models;
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
    public class StaffProductFacadeTests
    {
        public HttpClient client;
        public Mock<IHttpClientFactory> mockFactory;
        public Mock<HttpClient> mockClient;
        public Mock<HttpMessageHandler> mockHandler;
        public IStaffProductFacade facade;
        private IConfiguration config;
        private List<StockReductionDto> stockReductions;
        private Mock<IHttpHandler> mockHttpHandler;
        private string staffProductUriValue = "/api/customerstockorders/";
        private string staffAuthServerUrlKeyValue = "StaffAuthServerUrl";
        private string staffProductApiKeyValue = "StaffProductAPI";
        private string staffProductScopeKeyValue = "StaffProductScope";
        Uri expectedUri = new Uri("http://test/api/customerstockorders/");

        private void SetupStockReductions()
        {
            stockReductions = new List<StockReductionDto>
            {
                new StockReductionDto { ProductId = 1, Quantity = 2},
                new StockReductionDto { ProductId = 2, Quantity = 4}
            };
        }

        private void SetupConfig(string staffProductUri = null, string staffAuthUrlKey = null, string? staffProductAPIKey = null,
            string? staffProductScope = null)
        {
            var myConfiguration = new Dictionary<string, string>
             {
                {"StaffAuthServerUrlKey", staffAuthUrlKey??staffAuthServerUrlKeyValue},
                {"StaffProductAPIKey", staffProductAPIKey??staffProductApiKeyValue},
                {"StaffProductScopeKey", staffProductScope??staffProductScopeKeyValue},
                {"StaffProductUri" , staffProductUri?? staffProductUriValue },
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

        private void DefaultSetupRealHttpClient(HttpStatusCode statusCode)
        {
            SetupStockReductions();
            var expectedResult = new HttpResponseMessage
            {
                StatusCode = statusCode
            };
            SetMockMessageHandler(expectedResult);
            SetupRealHttpClient(expectedResult);
            SetupHttpFactoryMock(client);
            SetupConfig();
            SetupHttpHandlerMock();
            facade = new StaffProductFacade(config, mockHttpHandler.Object);
            SetupConfig();
        }

        [Fact]
        public async Task UpdateStock_ShouldReturnTrue()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.OK);

            //Act
            var result = await facade.UpdateStock(stockReductions);

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
            mockHttpHandler.Verify(m => m.GetClient(staffAuthServerUrlKeyValue, staffProductApiKeyValue,
                staffProductScopeKeyValue), Times.Once);
        }

        [Fact]
        public async Task UpdateStock_NotFound_ShouldReturnFalse()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.NotFound);

            //Act
            var result = await facade.UpdateStock(stockReductions);

            //Assert
            Assert.True(false == result);
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                 (req => req.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Once(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Post && req.RequestUri == expectedUri), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Put), ItExpr.IsAny<CancellationToken>());
            mockHandler.Protected().Verify("SendAsync", Times.Never(), ItExpr.Is<HttpRequestMessage>
                (req => req.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>());
            mockHttpHandler.Verify(m => m.GetClient(staffAuthServerUrlKeyValue, staffProductApiKeyValue,
                staffProductScopeKeyValue), Times.Once);
        }

        [Fact]
        public async Task UpdateStock_Null_ShouldReturnFalse()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.OK);

            //Act
            var result = await facade.UpdateStock(null);

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
            DefaultSetupRealHttpClient(HttpStatusCode.OK);

            //Act
            var result = await facade.UpdateStock(new List<StockReductionDto>());

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
            staffProductUriValue = null;
            DefaultSetupRealHttpClient(HttpStatusCode.OK);

            //Act
            var result = await facade.UpdateStock(new List<StockReductionDto>());

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
            staffProductUriValue = "";
            DefaultSetupRealHttpClient(HttpStatusCode.OK);

            //Act
            var result = await facade.UpdateStock(new List<StockReductionDto>());

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
            DefaultSetupRealHttpClient(HttpStatusCode.OK);

            //Act
            var result = await facade.UpdateStock(new List<StockReductionDto>());

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
            DefaultSetupRealHttpClient(HttpStatusCode.OK);

            //Act
            var result = await facade.UpdateStock(new List<StockReductionDto>());

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
            staffProductApiKeyValue = null;
            DefaultSetupRealHttpClient(HttpStatusCode.OK);

            //Act
            var result = await facade.UpdateStock(new List<StockReductionDto>());

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
        public async Task UpdateStock_ApiKeyEmpty_ShouldReturnFalse()
        {
            //Arrange
            staffProductApiKeyValue = "";
            DefaultSetupRealHttpClient(HttpStatusCode.OK);

            //Act
            var result = await facade.UpdateStock(new List<StockReductionDto>());

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
        public async Task UpdateStock_ScopeNull_ShouldReturnFalse()
        {
            //Arrange
            staffProductScopeKeyValue = null;
            DefaultSetupRealHttpClient(HttpStatusCode.OK);

            //Act
            var result = await facade.UpdateStock(new List<StockReductionDto>());

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
        public async Task UpdateStock_ScopeEmpty_ShouldReturnFalse()
        {
            //Arrange
            staffProductScopeKeyValue = "";
            DefaultSetupRealHttpClient(HttpStatusCode.OK);

            //Act
            var result = await facade.UpdateStock(new List<StockReductionDto>());

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
