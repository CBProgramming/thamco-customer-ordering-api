using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Order.Repository;
using Order.Repository.Models;
using OrderData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CustomerOrderingService.UnitTests
{
    public class RepositoryTests
    {
        public BasketItemRepoModel setupStandardBasketItemModel()
        {
            return new BasketItemRepoModel
            {
                CustomerId = 1,
                ProductId = 1,
                Price = 1.00,
                Quantity = 1,
                ProductName = "Fake name"
            };
        }

        private IMapper SetupMapper()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            }).CreateMapper();
        }

        [Fact]
        public async Task AddItemToBasket_ShouldTrue()
        {
            //Arrange
            var mapper = SetupMapper();
            var basketItem = setupStandardBasketItemModel();

            var products = new List<Product>
            {
                new Product { ProductId = 1, Name = "Name", Price = 1 }
            }.AsQueryable();

            var mockBasketItems = new Mock<DbSet<BasketItem>>();

            var mockProducts = new Mock<DbSet<Product>>();
            mockProducts.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(products.Provider);
            mockProducts.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(products.Expression);
            mockProducts.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(products.ElementType);
            mockProducts.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(products.GetEnumerator());
            var mockContext = new Mock<OrderDb>();
            
            mockContext.Setup(m => m.BasketItems).Returns(mockBasketItems.Object);
            mockContext.Setup(m => m.Products).Returns(mockProducts.Object);
            var repository = new OrderRepository(mockContext.Object, mapper);

            //Act
            var result = repository.AddBasketItem(basketItem);

            //Assert
            Assert.NotNull(result);
            mockBasketItems.Verify(m => m.Add(It.IsAny<BasketItem>()), Times.Once());
        }
    }
}
