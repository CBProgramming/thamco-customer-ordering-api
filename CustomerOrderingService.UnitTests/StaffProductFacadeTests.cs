﻿using Microsoft.Extensions.Configuration;
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

        private void SetupStockReductions()
        {
            stockReductions = new List<StockReductionDto>
            {
                new StockReductionDto { ProductId = 1, Quantity = 2},
                new StockReductionDto { ProductId = 2, Quantity = 4}
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
            SetupStockReductions();
            var expectedResult = new HttpResponseMessage
            {
                StatusCode = statusCode
            };
            SetMockMessageHandler(expectedResult);
            SetupRealHttpClient(expectedResult);
            SetupHttpFactoryMock(client);
            SetupConfig();
            facade = new StaffProductFacade(mockFactory.Object, config);
            SetupConfig();
        }

        [Fact]
        public async Task UpdateStock_ShouldReturnTrue()
        {
            //Arrange
            DefaultSetupRealHttpClient(HttpStatusCode.OK);
            var expectedUri = new Uri("http://test/fake/Uri");

            //Act
            var result = await facade.UpdateStock(stockReductions);

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
            var result = await facade.UpdateStock(stockReductions);

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
            var result = await facade.UpdateStock(null);

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
