﻿using Microsoft.Extensions.Configuration;
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
        /*public HttpClient client;
        public Mock<IHttpClientFactory> mockFactory;
        public Mock<HttpClient> mockClient;
        public Mock<HttpMessageHandler> mockHandler;
        public IReviewFacade facade;
        private IConfiguration config;
        private PurchaseDto purchases;

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

        private void SetupConfig()
        {
            var myConfiguration = new Dictionary<string, string>
                {{"ClientId", "clientId"},
                {"ClientSecret", "clientSecret"},
                {"CustomerAuthServerUrl", "https://fakeurl.com"},
                {"StaffAuthServerUrl", "https://fakeurl.com"},
                {"CustomerAccountUrl", "https://fakeurl.com"},
                {"InvoiceUrl", "https://fakeurl.com"},
                {"InvoiceUri", "fake/Uri"},
                {"StaffProductUrl", "https://fakeurl.com"},
                {"StaffProductUri", "/fake/Uri"},
                {"ReviewUrl", "https://fakeurl.com"},
                {"ReviewProductUri", "fake/Uri"}};

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
            facade = new ReviewFacade(mockFactory.Object, config);
            SetupConfig();
        }

        [Fact]
        public async Task UpdateStock_ShouldReturnTrue()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/fake/Uri");

            //Act
            var result = await facade.NewPurchases(purchases);

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
            var result = await facade.NewPurchases(purchases);

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
            var result = await facade.NewPurchases(null);

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
            purchases.OrderedItems = new List<ProductDto>();

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
        }*/
    }
}
