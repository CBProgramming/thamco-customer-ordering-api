using HttpManager;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Review.Facade;
using Review.Facade.Models;
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
    public class ReviewFacadeTests
    {
        public HttpClient client;
        public Mock<IHttpClientFactory> mockFactory;
        public Mock<HttpClient> mockClient;
        public Mock<HttpMessageHandler> mockHandler;
        public IReviewFacade facade;
        private IConfiguration config;
        private PurchaseDto purchases;
        private Mock<IHttpHandler> mockHttpHandler;
        private string reviewUriValue = "/api/Product/";
        private string customerAuthServerUrlKeyValue = "CustomerAuthServerUrl";
        private string reviewApiKeyValue = "ReviewAPI";
        private string reviewScopeKeyValue = "ReviewScope";
        Uri expectedUri = new Uri("http://test/api/Product/");

        private void SetupOrder()
        {
            purchases = new PurchaseDto
            {
                CustomerAuthId = "fakeAuthId",
                CustomerId = 1,
                OrderedItems = new List<ProductDto>
                {
                    new ProductDto{ ProductId = 1},
                    new ProductDto{ ProductId = 2},
                    new ProductDto{ ProductId = 3}
                }
            };
        }

        private void SetupConfig(string reviewUri = null, string customerAuthUrlKey = null, string? reviewAPIKey = null,
            string? reviewScope = null)
        {
            var myConfiguration = new Dictionary<string, string>
             {
                {"CustomerAuthServerUrlKey", customerAuthUrlKey??customerAuthServerUrlKeyValue},
                {"ReviewAPIKey", reviewAPIKey??reviewApiKeyValue},
                {"ReviewScopeKey", reviewScope??reviewScopeKeyValue},
                {"ReviewUri" , reviewUri?? reviewUriValue },
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
            facade = new ReviewFacade(config, mockHttpHandler.Object);
            SetupConfig();
        }

        [Fact]
        public async Task UpdateStock_ShouldReturnTrue()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.OK);

            //Act
            var result = await facade.NewPurchases(purchases);

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
            mockHttpHandler.Verify(m => m.GetClient(customerAuthServerUrlKeyValue, reviewApiKeyValue,
                reviewScopeKeyValue), Times.Once);
        }

        [Fact]
        public async Task UpdateStock_NotFound_ShouldReturnFalse()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.NotFound);

            //Act
            var result = await facade.NewPurchases(purchases);

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
            mockHttpHandler.Verify(m => m.GetClient(customerAuthServerUrlKeyValue, reviewApiKeyValue,
                reviewScopeKeyValue), Times.Once);
        }

        [Fact]
        public async Task UpdateStock_Null_ShouldReturnFalse()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.OK);

            //Act
            var result = await facade.NewPurchases(null);

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
            purchases.OrderedItems = new List<ProductDto>();

            //Act
            var result = await facade.NewPurchases(purchases);

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
            reviewUriValue = null;
            DefaultSetupRealHttpClient(HttpStatusCode.OK);

            //Act
            var result = await facade.NewPurchases(purchases);

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
            reviewUriValue = "";
            DefaultSetupRealHttpClient(HttpStatusCode.OK);

            //Act
            var result = await facade.NewPurchases(purchases);

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
            customerAuthServerUrlKeyValue = null;
            DefaultSetupRealHttpClient(HttpStatusCode.OK);

            //Act
            var result = await facade.NewPurchases(purchases);

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
            customerAuthServerUrlKeyValue = "";
            DefaultSetupRealHttpClient(HttpStatusCode.OK);

            //Act
            var result = await facade.NewPurchases(purchases);

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
            reviewApiKeyValue = null;
            DefaultSetupRealHttpClient(HttpStatusCode.OK);

            //Act
            var result = await facade.NewPurchases(purchases);

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
            reviewApiKeyValue = "";
            DefaultSetupRealHttpClient(HttpStatusCode.OK);

            //Act
            var result = await facade.NewPurchases(purchases);

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
            reviewScopeKeyValue = null;
            DefaultSetupRealHttpClient(HttpStatusCode.OK);

            //Act
            var result = await facade.NewPurchases(purchases);

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
        public async Task UpdateStock_ScopeKeyEmpty_ShouldReturnFalse()
        {
            //Arrange
            reviewScopeKeyValue = "";
            DefaultSetupRealHttpClient(HttpStatusCode.OK);

            //Act
            var result = await facade.NewPurchases(purchases);

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
