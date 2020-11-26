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
        private Mock<HttpMessageHandler> MockMessageHandler(HttpResponseMessage expected)
        {
            var mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expected)
                .Verifiable();
            return mock;
        }

        private HttpClient SetupHttpClient(HttpResponseMessage expected)
        {
            var client = new HttpClient(MockMessageHandler(expected).Object);
            client.BaseAddress = new Uri("http://test");
            return client;
        }
        private Mock<IHttpClientFactory> CreateHttpFactoryMock(HttpClient client)
        {
            var factoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client).Verifiable();
            return factoryMock;
        }

        private IStaffProductFacade mockFacade(IHttpClientFactory mockFactory)
        {
            return new StaffProductFacade(mockFactory);
        }

        [Fact]
        public async Task UpdateStock_ShouldReturnTrue()
        {
            //Arrange
            var expectedResult = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };
            List<StockReductionDto> stockReductions = new List<StockReductionDto>
            {
                new StockReductionDto { ProductId = 1, Quantity = 2},
                new StockReductionDto { ProductId = 2, Quantity = 4}
            };
            var client = SetupHttpClient(expectedResult);
            var factory = CreateHttpFactoryMock(client);
            var facade = new StaffProductFacade(factory.Object);

            //Act
            var result = await facade.UpdateStock(stockReductions);

            //Assert
            Assert.True(true == result);
        }
    }
}
