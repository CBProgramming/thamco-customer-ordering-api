using AutoMapper;
using CustomerOrderingService.Controllers;
using CustomerOrderingService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Order.Repository;
using Order.Repository.Data;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace CustomerOrderingService.UnitTests
{
    public class ProductControllerTests
    {
        private ProductDto productDto;
        private ProductRepoModel productRepoModel;
        private FakeOrderRepository fakeRepo;
        private Mock<IOrderRepository> mockRepo;
        private IMapper mapper;
        private ILogger<ProductController> logger;
        private ProductController controller;

        private void SetStandardProductDto()
        {
            productDto = new ProductDto
            {
                ProductId = 1,
                Name = "Product 1",
                Price = 1.99
            };
        }

        private void SetStandardProductRepoModel()
        {
            productRepoModel = new ProductRepoModel
            {
                ProductId = 1,
                Name = "Product 1",
                Price = 1.99
            };
        }

        private void SetFakeRepo(ProductRepoModel product)
        {
            fakeRepo = new FakeOrderRepository
            {
                Product = product
            };
            fakeRepo.Products = new List<ProductRepoModel>
            {
                product
            };
        }

        private void SetMapper()
        {
            mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new UserProfile());
            }).CreateMapper();
        }

        private void SetLogger()
        {
            logger = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider()
                .GetService<ILoggerFactory>()
                .CreateLogger<ProductController>();
        }

        private void SetMockProductRepo(bool productExists = true, bool succeeds = true)
        {
            mockRepo = new Mock<IOrderRepository>(MockBehavior.Strict);
            mockRepo.Setup(repo => repo.ProductExists(It.IsAny<int>())).ReturnsAsync(productExists && succeeds).Verifiable();
            mockRepo.Setup(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>())).ReturnsAsync(succeeds).Verifiable();
            mockRepo.Setup(repo => repo.EditProduct(It.IsAny<ProductRepoModel>())).ReturnsAsync(succeeds).Verifiable();
            mockRepo.Setup(repo => repo.DeleteProduct(It.IsAny<int>())).ReturnsAsync(succeeds).Verifiable();
        }

        private void SetupApi(ProductController controller)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim("client_id","customer_product_app")
                                   }, "TestAuth"));
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        }

        private void DefaultSetup(bool withMocks = false, bool setupUser = true, bool setupApi = false)
        {
            SetStandardProductDto();
            SetStandardProductRepoModel();
            SetFakeRepo(productRepoModel);
            SetMapper();
            SetLogger();
            SetMockProductRepo();
            if (!withMocks)
            {
                controller = new ProductController(logger, fakeRepo, mapper);
            }
            else
            {
                controller = new ProductController(logger, mockRepo.Object, mapper);
            }
            SetupApi(controller);
        }

        [Fact]
        public async void EditProduct_ProductDoesntExist_ShouldOkCreatingNewProduct()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Product = null;

            //Act
            var result = await controller.Put(productDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void EditProduct_ProductDoesntExist_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockProductRepo(productExists: false);
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);

            //Act
            var result = await controller.Put(productDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(productDto.ProductId), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void EditProduct__ShouldOkWithEditedDetails()
        {
            //Arrange
            DefaultSetup();
            productDto.Name = "New Name";
            productDto.Price = 2.99;
            productDto.Quantity = 5;
            int oldStock = productRepoModel.Quantity;

            //Act
            var result = await controller.Put(productDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Product);
            Assert.Equal(fakeRepo.Product.ProductId, productDto.ProductId);
            Assert.Equal(fakeRepo.Product.Name, productDto.Name);
            Assert.Equal(fakeRepo.Product.Price, productDto.Price);
            Assert.Equal(fakeRepo.Product.Quantity, productDto.Quantity + oldStock);
        }

        [Fact]
        public async void EditProduct_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockProductRepo();
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);

            //Act
            var result = await controller.Put(productDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(productDto.ProductId), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }


        [Fact]
        public async void EditProduct_NullProduct_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await controller.Put(null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void EditProduct_NullProduct_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockProductRepo();
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);

            //Act
            var result = await controller.Put(null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(productDto.ProductId), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void EditProduct__RepoFailure_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.AutoFails = true;

            //Act
            var result = await controller.Put(productDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void CreateProduct_ProductDoesntExist_ShouldOkCreatingNewProduct()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Product = null;

            //Act
            var result = await controller.Post(productDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void CreateProduct_ProductDoesntExist_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockProductRepo(productExists: false);
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);

            //Act
            var result = await controller.Post(productDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(productDto.ProductId), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void CreateProduct__ShouldOkWithEditedDetails()
        {
            //Arrange
            DefaultSetup();
            productDto.Name = "New Name";
            productDto.Price = 2.99;
            productDto.Quantity = 5;
            int oldStock = productRepoModel.Quantity;

            //Act
            var result = await controller.Post(productDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Product);
            Assert.Equal(fakeRepo.Product.ProductId, productDto.ProductId);
            Assert.Equal(fakeRepo.Product.Name, productDto.Name);
            Assert.Equal(fakeRepo.Product.Price, productDto.Price);
            Assert.Equal(fakeRepo.Product.Quantity, productDto.Quantity + oldStock);
        }

        [Fact]
        public async void CreateProduct_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockProductRepo();
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);

            //Act
            var result = await controller.Post(productDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(productDto.ProductId), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }


        [Fact]
        public async void CreateProduct_NullProduct_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await controller.Post(null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void CreateProduct_NullProduct_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockProductRepo();
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);

            //Act
            var result = await controller.Post(null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(productDto.ProductId), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void CreateProduct__RepoFailure_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.AutoFails = true;

            //Act
            var result = await controller.Post(productDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void DeleteProduct_ProductDoesntExist_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Product = null;

            //Act
            var result = await controller.Delete(productDto.ProductId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void DeleteProduct_ProductDoesntExist_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockProductRepo(productExists: false);
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);

            //Act
            var result = await controller.Delete(productDto.ProductId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(productDto.ProductId), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void DeleteProduct__ShouldOk()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await controller.Delete(productDto.ProductId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Null(fakeRepo.Product);
        }

        [Fact]
        public async void DeleteProduct_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockProductRepo();
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);

            //Act
            var result = await controller.Delete(productDto.ProductId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(productDto.ProductId), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async void DeleteProduct__RepoFailure_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.AutoFails = true;

            //Act
            var result = await controller.Delete(productDto.ProductId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }
    }
}
