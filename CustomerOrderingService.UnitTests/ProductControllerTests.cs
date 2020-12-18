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
        private ProductDto productDto1, productDto2;
        private List<ProductDto> products;
        private ProductRepoModel productRepoModel;
        private FakeOrderRepository fakeRepo;
        private Mock<IOrderRepository> mockRepo;
        private IMapper mapper;
        private ILogger<ProductController> logger;
        private ProductController controller;

        private void SetStandardProductDtos(bool singleDto)
        {
            productDto1 = new ProductDto
            {
                ProductId = 1,
                Name = "Product 1",
                Price = 1.99,
                Quantity = 2
            };
            productDto2 = new ProductDto
            {
                ProductId = 2,
                Name = "Product 2",
                Price = 2.99,
                Quantity = 3
            };
            products = new List<ProductDto>();
            products.Add(productDto1);
            if (!singleDto)
            {
                products.Add(productDto2);
            }
        }

        private void SetStandardProductRepoModel()
        {
            productRepoModel = new ProductRepoModel
            {
                ProductId = 1,
                Name = "Product 1",
                Price = 1.99,
                Quantity = 2
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

        private void DefaultSetup(bool withMocks = false, bool setupUser = true, bool setupApi = false, bool singleDto = true)
        {
            SetStandardProductDtos(singleDto);
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
        public async void EditProducts_ProductDoesntExist_ShouldOkCreatingNewProduct()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Products = new List<ProductRepoModel>();

            //Act
            var result = await controller.Put(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[0].ProductId, productDto1.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productDto1.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productDto1.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productDto1.Quantity);
            Assert.True(1 == fakeRepo.Products.Count);
        }

        [Fact]
        public async void EditProducts_ProductDoesntExist_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockProductRepo(productExists: false);
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);

            //Act
            var result = await controller.Put(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(productDto1.ProductId), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void EditProducts__ShouldOkWithEditedDetails()
        {
            //Arrange
            DefaultSetup();
            productDto1.Name = "New Name";
            productDto1.Price = 2.99;
            productDto1.Quantity = 5;
            int oldStock = productRepoModel.Quantity;

            //Act
            var result = await controller.Put(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Product);
            Assert.Equal(fakeRepo.Product.ProductId, productDto1.ProductId);
            Assert.Equal(fakeRepo.Product.Name, productDto1.Name);
            Assert.Equal(fakeRepo.Product.Price, productDto1.Price);
            Assert.Equal(fakeRepo.Product.Quantity, productDto1.Quantity + oldStock);
            Assert.True(1 == fakeRepo.Products.Count);
        }

        [Fact]
        public async void EditProducts_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockProductRepo();
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);

            //Act
            var result = await controller.Put(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(productDto1.ProductId), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }


        [Fact]
        public async void EditProducts_NullProducts_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await controller.Put(null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[0].ProductId, productRepoModel.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productRepoModel.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productRepoModel.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productRepoModel.Quantity);
            Assert.True(1 == fakeRepo.Products.Count);
        }

        [Fact]
        public async void EditProducts_NullProducts_CheckMocks()
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
            mockRepo.Verify(repo => repo.ProductExists(productDto1.ProductId), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void EditProducts_EmptyProducts_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await controller.Put(new List<ProductDto>());

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[0].ProductId, productRepoModel.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productRepoModel.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productRepoModel.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productRepoModel.Quantity);
            Assert.True(1 == fakeRepo.Products.Count);
        }

        [Fact]
        public async void EditProducts_EmptyProducts_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockProductRepo();
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);

            //Act
            var result = await controller.Put(new List<ProductDto>());

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(productDto1.ProductId), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void EditProducts_ProductsContainsNull_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup();
            products = new List<ProductDto>();
            products.Add(null);

            //Act
            var result = await controller.Put(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[0].ProductId, productRepoModel.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productRepoModel.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productRepoModel.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productRepoModel.Quantity);
            Assert.True(1 == fakeRepo.Products.Count);
        }

        [Fact]
        public async void EditProducts_ProductsContainsNull_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockProductRepo();
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);
            products = new List<ProductDto>();
            products.Add(null);

            //Act
            var result = await controller.Put(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(productDto1.ProductId), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void EditProducts_ProductsContainsNullAndValidProduct_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup();
            products = new List<ProductDto>();
            products.Add(productDto1);
            products.Add(null);

            //Act
            var result = await controller.Put(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[0].ProductId, productRepoModel.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productRepoModel.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productRepoModel.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productRepoModel.Quantity);
            Assert.True(1 == fakeRepo.Products.Count);
        }

        [Fact]
        public async void EditProducts_ProductsContainsNullAndValidProduct_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockProductRepo();
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);
            products = new List<ProductDto>();
            products.Add(productDto1);
            products.Add(null);

            //Act
            var result = await controller.Put(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(productDto1.ProductId), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void EditProducts__RepoFailure_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.AutoFails = true;

            //Act
            var result = await controller.Put(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[0].ProductId, productRepoModel.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productRepoModel.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productRepoModel.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productRepoModel.Quantity);
            Assert.True(1 == fakeRepo.Products.Count);
        }

        [Fact]
        public async void EditProducts_BothProductDontExist_ShouldOkCreatingNewProduct()
        {
            //Arrange
            DefaultSetup(singleDto: false);
            fakeRepo.Products = new List<ProductRepoModel>();

            //Act
            var result = await controller.Put(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[0].ProductId, productDto1.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productDto1.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productDto1.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productDto1.Quantity);
            Assert.Equal(fakeRepo.Products[1].ProductId, productDto2.ProductId);
            Assert.Equal(fakeRepo.Products[1].Name, productDto2.Name);
            Assert.Equal(fakeRepo.Products[1].Price, productDto2.Price);
            Assert.Equal(fakeRepo.Products[1].Quantity, productDto2.Quantity);
            Assert.True(2 == fakeRepo.Products.Count);
        }

        [Fact]
        public async void EditProducts_BothProductDontExist_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, singleDto: false);
            SetMockProductRepo(productExists: false);
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);

            //Act
            var result = await controller.Put(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Exactly(2));
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Exactly(2));
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void EditProducts__OneExistingOneNewShouldOkWithEditedDetails()
        {
            //Arrange
            DefaultSetup(singleDto: false);
            productDto1.Name = "New Name";
            productDto1.Price = 2.99;
            productDto1.Quantity = 5;
            int oldStock = productRepoModel.Quantity;

            //Act
            var result = await controller.Put(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Product);
            Assert.Equal(fakeRepo.Products[0].ProductId, productDto1.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productDto1.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productDto1.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productDto1.Quantity + oldStock);
            Assert.Equal(fakeRepo.Products[1].ProductId, productDto2.ProductId);
            Assert.Equal(fakeRepo.Products[1].Name, productDto2.Name);
            Assert.Equal(fakeRepo.Products[1].Price, productDto2.Price);
            Assert.Equal(fakeRepo.Products[1].Quantity, productDto2.Quantity);
            Assert.Equal(2, fakeRepo.Products.Count);
        }

        [Fact]
        public async void EditProducts__BothExist_ShouldOkWithEditedDetails()
        {
            //Arrange
            DefaultSetup(singleDto: false);
            productDto1.Name = "New Name";
            productDto1.Price = 2.99;
            productDto1.Quantity = 5;
            int oldStock = productRepoModel.Quantity;
            int product2Stock = 3;
            fakeRepo.Products.Add(new ProductRepoModel
            {
                ProductId = 2,
                Name = "Anything",
                Quantity = product2Stock,
                Price = 3.99
            });

            //Act
            var result = await controller.Put(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Product);
            Assert.Equal(fakeRepo.Products[0].ProductId, productDto1.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productDto1.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productDto1.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productDto1.Quantity + oldStock);
            Assert.Equal(fakeRepo.Products[1].ProductId, productDto2.ProductId);
            Assert.Equal(fakeRepo.Products[1].Name, productDto2.Name);
            Assert.Equal(fakeRepo.Products[1].Price, productDto2.Price);
            Assert.Equal(fakeRepo.Products[1].Quantity, productDto2.Quantity + product2Stock);
            Assert.Equal(2, fakeRepo.Products.Count);
        }

        [Fact]
        public async void EditTwoProducts_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, singleDto: false);
            SetMockProductRepo();
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);

            //Act
            var result = await controller.Put(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Exactly(2));
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Exactly(2));
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void EditTwoProducts__RepoFailure_ShouldNotFound()
        {
            //Arrange
            DefaultSetup(singleDto: false);
            fakeRepo.AutoFails = true;

            //Act
            var result = await controller.Put(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[0].ProductId, productRepoModel.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productRepoModel.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productRepoModel.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productRepoModel.Quantity);
            Assert.True(1 == fakeRepo.Products.Count);
        }

        [Fact]
        public async void EditProducts_BothProductDontExist_RepoFailsOnce_ShouldOkCreatingNewProduct()
        {
            //Arrange
            DefaultSetup(singleDto: false);
            fakeRepo.Products = new List<ProductRepoModel>();
            fakeRepo.FailureAmount = 1;

            //Act
            var result = await controller.Put(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[1].ProductId, productDto1.ProductId);
            Assert.Equal(fakeRepo.Products[1].Name, productDto1.Name);
            Assert.Equal(fakeRepo.Products[1].Price, productDto1.Price);
            Assert.Equal(fakeRepo.Products[1].Quantity, productDto1.Quantity);
            Assert.Equal(fakeRepo.Products[0].ProductId, productDto2.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productDto2.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productDto2.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productDto2.Quantity);
            Assert.Equal(2, fakeRepo.Products.Count);
        }

        [Fact]
        public async void EditProducts__OneExistingOneNew_RepoFailsOnce_ShouldOkWithEditedDetails()
        {
            //Arrange
            DefaultSetup(singleDto: false);
            productDto1.Name = "New Name";
            productDto1.Price = 2.99;
            productDto1.Quantity = 5;
            int oldStock = productRepoModel.Quantity;
            fakeRepo.FailureAmount = 1;

            //Act
            var result = await controller.Put(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Product);
            Assert.Equal(fakeRepo.Products[0].ProductId, productDto1.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productDto1.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productDto1.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productDto1.Quantity + oldStock);
            Assert.Equal(fakeRepo.Products[1].ProductId, productDto2.ProductId);
            Assert.Equal(fakeRepo.Products[1].Name, productDto2.Name);
            Assert.Equal(fakeRepo.Products[1].Price, productDto2.Price);
            Assert.Equal(fakeRepo.Products[1].Quantity, productDto2.Quantity);
            Assert.Equal(2, fakeRepo.Products.Count);
        }

        [Fact]
        public async void EditProducts__BothExist_RepoFailsOnce_ShouldOkWithEditedDetails()
        {
            //Arrange
            DefaultSetup(singleDto: false);
            productDto1.Name = "New Name";
            productDto1.Price = 2.99;
            productDto1.Quantity = 5;
            int oldStock = productRepoModel.Quantity;
            int product2Stock = 3;
            fakeRepo.Products.Add(new ProductRepoModel
            {
                ProductId = 2,
                Name = "Anything",
                Quantity = product2Stock,
                Price = 3.99
            });
            fakeRepo.FailureAmount = 1;

            //Act
            var result = await controller.Put(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Product);
            Assert.Equal(fakeRepo.Products[0].ProductId, productDto1.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productDto1.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productDto1.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productDto1.Quantity + oldStock);
            Assert.Equal(fakeRepo.Products[1].ProductId, productDto2.ProductId);
            Assert.Equal(fakeRepo.Products[1].Name, productDto2.Name);
            Assert.Equal(fakeRepo.Products[1].Price, productDto2.Price);
            Assert.Equal(fakeRepo.Products[1].Quantity, productDto2.Quantity + product2Stock);
            Assert.Equal(2, fakeRepo.Products.Count);
        }

        [Fact]
        public async void EditProducts_ThreeProductsDontExist_RepoFailsTwice_ShouldOkCreatingNewProducts()
        {
            //Arrange
            DefaultSetup(singleDto: false);
            fakeRepo.Products = new List<ProductRepoModel>();
            fakeRepo.FailureAmount = 2;
            products.Add(new ProductDto
            {
                ProductId = 3,
                Name = "Something",
                Quantity = 7,
                Price = 9.99
            });

            //Act
            var result = await controller.Put(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[1].ProductId, productDto1.ProductId);
            Assert.Equal(fakeRepo.Products[1].Name, productDto1.Name);
            Assert.Equal(fakeRepo.Products[1].Price, productDto1.Price);
            Assert.Equal(fakeRepo.Products[1].Quantity, productDto1.Quantity);
            Assert.Equal(fakeRepo.Products[2].ProductId, productDto2.ProductId);
            Assert.Equal(fakeRepo.Products[2].Name, productDto2.Name);
            Assert.Equal(fakeRepo.Products[2].Price, productDto2.Price);
            Assert.Equal(fakeRepo.Products[2].Quantity, productDto2.Quantity);
            Assert.Equal(fakeRepo.Products[0].ProductId, products[2].ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, products[2].Name);
            Assert.Equal(fakeRepo.Products[0].Price, products[2].Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, products[2].Quantity);
            Assert.Equal(3, fakeRepo.Products.Count);
        }

        [Fact]
        public async void EditProducts_ThreeProductsDontExist_RepoFailsThrice_ShouldNotFound()
        {
            //Arrange
            DefaultSetup(singleDto: false);
            fakeRepo.FailureAmount = 3;
            products.Add(new ProductDto
            {
                ProductId = 3,
                Name = "Something",
                Quantity = 7,
                Price = 9.99
            });

            //Act
            var result = await controller.Put(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[0].ProductId, productRepoModel.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productRepoModel.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productRepoModel.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productRepoModel.Quantity);
            Assert.True(1 == fakeRepo.Products.Count);
        }

        [Fact]
        public async void CreateProducts_ProductDoesntExist_ShouldOkCreatingNewProduct()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Products = new List<ProductRepoModel>();

            //Act
            var result = await controller.Post(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[0].ProductId, productDto1.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productDto1.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productDto1.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productDto1.Quantity);
            Assert.True(1 == fakeRepo.Products.Count);
        }

        [Fact]
        public async void CreateProducts_ProductDoesntExist_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockProductRepo(productExists: false);
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);

            //Act
            var result = await controller.Post(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(productDto1.ProductId), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void CreateProducts__ShouldOkWithEditedDetails()
        {
            //Arrange
            DefaultSetup();
            productDto1.Name = "New Name";
            productDto1.Price = 2.99;
            productDto1.Quantity = 5;
            int oldStock = productRepoModel.Quantity;

            //Act
            var result = await controller.Post(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Product);
            Assert.Equal(fakeRepo.Products[0].ProductId, productDto1.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productDto1.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productDto1.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productDto1.Quantity + oldStock);
            Assert.True(1 == fakeRepo.Products.Count);
        }

        [Fact]
        public async void CreateProducts_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockProductRepo();
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);

            //Act
            var result = await controller.Post(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(productDto1.ProductId), Times.Once);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }


        [Fact]
        public async void CreateProducts_NullProduct_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await controller.Post(null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[0].ProductId, productRepoModel.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productRepoModel.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productRepoModel.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productRepoModel.Quantity);
            Assert.True(1 == fakeRepo.Products.Count);
        }

        [Fact]
        public async void CreateProducts_EmptyProducts_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await controller.Post(new List<ProductDto>());

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[0].ProductId, productRepoModel.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productRepoModel.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productRepoModel.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productRepoModel.Quantity);
            Assert.True(1 == fakeRepo.Products.Count);
        }

        [Fact]
        public async void CreateProducts_EmptyProducts_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockProductRepo();
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);

            //Act
            var result = await controller.Post(new List<ProductDto>());

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(productDto1.ProductId), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void CreateProducts_ProductsContainsNull_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup();
            products = new List<ProductDto>();
            products.Add(null);

            //Act
            var result = await controller.Post(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[0].ProductId, productRepoModel.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productRepoModel.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productRepoModel.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productRepoModel.Quantity);
            Assert.True(1 == fakeRepo.Products.Count);
        }

        [Fact]
        public async void CreateProducts_ProductsContainsNull_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockProductRepo();
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);
            products = new List<ProductDto>();
            products.Add(null);

            //Act
            var result = await controller.Post(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(productDto1.ProductId), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void CreateProducts_ProductsContainsNullAndValidProduct_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup();
            products = new List<ProductDto>();
            products.Add(productDto1);
            products.Add(null);

            //Act
            var result = await controller.Post(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[0].ProductId, productRepoModel.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productRepoModel.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productRepoModel.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productRepoModel.Quantity);
            Assert.True(1 == fakeRepo.Products.Count);
        }

        [Fact]
        public async void CreateProducts_ProductsContainsNullAndValidProduct_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockProductRepo();
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);
            products = new List<ProductDto>();
            products.Add(null);
            products.Add(productDto1);

            //Act
            var result = await controller.Post(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(productDto1.ProductId), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void CreateProducts_NullProduct_CheckMocks()
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
            mockRepo.Verify(repo => repo.ProductExists(productDto1.ProductId), Times.Never);
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void CreateProducts__RepoFailure_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.AutoFails = true;

            //Act
            var result = await controller.Post(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[0].ProductId, productRepoModel.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productRepoModel.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productRepoModel.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productRepoModel.Quantity);
            Assert.True(1 == fakeRepo.Products.Count);
        }

        [Fact]
        public async void CreateProducts_BothProductDontExist_ShouldOkCreatingNewProduct()
        {
            //Arrange
            DefaultSetup(singleDto: false);
            fakeRepo.Products = new List<ProductRepoModel>();

            //Act
            var result = await controller.Post(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[0].ProductId, productDto1.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productDto1.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productDto1.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productDto1.Quantity);
            Assert.Equal(fakeRepo.Products[1].ProductId, productDto2.ProductId);
            Assert.Equal(fakeRepo.Products[1].Name, productDto2.Name);
            Assert.Equal(fakeRepo.Products[1].Price, productDto2.Price);
            Assert.Equal(fakeRepo.Products[1].Quantity, productDto2.Quantity);
            Assert.True(2 == fakeRepo.Products.Count);
        }

        [Fact]
        public async void CreateProducts_BothProductDontExist_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, singleDto: false);
            SetMockProductRepo(productExists: false);
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);

            //Act
            var result = await controller.Post(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Exactly(2));
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Exactly(2));
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void CreateProducts__OneExistingOneNewShouldOkWithEditedDetails()
        {
            //Arrange
            DefaultSetup(singleDto: false);
            productDto1.Name = "New Name";
            productDto1.Price = 2.99;
            productDto1.Quantity = 5;
            int oldStock = productRepoModel.Quantity;

            //Act
            var result = await controller.Post(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Product);
            Assert.Equal(fakeRepo.Products[0].ProductId, productDto1.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productDto1.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productDto1.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productDto1.Quantity + oldStock);
            Assert.Equal(fakeRepo.Products[1].ProductId, productDto2.ProductId);
            Assert.Equal(fakeRepo.Products[1].Name, productDto2.Name);
            Assert.Equal(fakeRepo.Products[1].Price, productDto2.Price);
            Assert.Equal(fakeRepo.Products[1].Quantity, productDto2.Quantity);
            Assert.Equal(2, fakeRepo.Products.Count);
        }

        [Fact]
        public async void CreateProducts__BothExist_ShouldOkWithEditedDetails()
        {
            //Arrange
            DefaultSetup(singleDto: false);
            productDto1.Name = "New Name";
            productDto1.Price = 2.99;
            productDto1.Quantity = 5;
            int oldStock = productRepoModel.Quantity;
            int product2Stock = 3;
            fakeRepo.Products.Add(new ProductRepoModel
            {
                ProductId = 2,
                Name = "Anything",
                Quantity = product2Stock,
                Price = 3.99
            });

            //Act
            var result = await controller.Post(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Product);
            Assert.Equal(fakeRepo.Products[0].ProductId, productDto1.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productDto1.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productDto1.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productDto1.Quantity + oldStock);
            Assert.Equal(fakeRepo.Products[1].ProductId, productDto2.ProductId);
            Assert.Equal(fakeRepo.Products[1].Name, productDto2.Name);
            Assert.Equal(fakeRepo.Products[1].Price, productDto2.Price);
            Assert.Equal(fakeRepo.Products[1].Quantity, productDto2.Quantity + product2Stock);
            Assert.Equal(2, fakeRepo.Products.Count);
        }

        [Fact]
        public async void CreateTwoProducts_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, singleDto: false);
            SetMockProductRepo();
            controller = new ProductController(logger, mockRepo.Object, mapper);
            SetupApi(controller);

            //Act
            var result = await controller.Post(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(It.IsAny<int>()), Times.Exactly(2));
            mockRepo.Verify(repo => repo.CreateProduct(It.IsAny<ProductRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditProduct(It.IsAny<ProductRepoModel>()), Times.Exactly(2));
            mockRepo.Verify(repo => repo.DeleteProduct(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void CreateTwoProducts__RepoFailure_ShouldNotFound()
        {
            //Arrange
            DefaultSetup(singleDto: false);
            fakeRepo.AutoFails = true;

            //Act
            var result = await controller.Post(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[0].ProductId, productRepoModel.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productRepoModel.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productRepoModel.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productRepoModel.Quantity);
            Assert.True(1 == fakeRepo.Products.Count);
        }

        [Fact]
        public async void CreateProducts_BothProductDontExist_RepoFailsOnce_ShouldOkCreatingNewProduct()
        {
            //Arrange
            DefaultSetup(singleDto: false);
            fakeRepo.Products = new List<ProductRepoModel>();
            fakeRepo.FailureAmount = 1;

            //Act
            var result = await controller.Post(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[1].ProductId, productDto1.ProductId);
            Assert.Equal(fakeRepo.Products[1].Name, productDto1.Name);
            Assert.Equal(fakeRepo.Products[1].Price, productDto1.Price);
            Assert.Equal(fakeRepo.Products[1].Quantity, productDto1.Quantity);
            Assert.Equal(fakeRepo.Products[0].ProductId, productDto2.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productDto2.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productDto2.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productDto2.Quantity);
            Assert.Equal(2, fakeRepo.Products.Count);
        }

        [Fact]
        public async void CreateProducts__OneExistingOneNew_RepoFailsOnce_ShouldOkWithEditedDetails()
        {
            //Arrange
            DefaultSetup(singleDto: false);
            productDto1.Name = "New Name";
            productDto1.Price = 2.99;
            productDto1.Quantity = 5;
            int oldStock = productRepoModel.Quantity;
            fakeRepo.FailureAmount = 1;

            //Act
            var result = await controller.Post(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Product);
            Assert.Equal(fakeRepo.Products[0].ProductId, productDto1.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productDto1.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productDto1.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productDto1.Quantity + oldStock);
            Assert.Equal(fakeRepo.Products[1].ProductId, productDto2.ProductId);
            Assert.Equal(fakeRepo.Products[1].Name, productDto2.Name);
            Assert.Equal(fakeRepo.Products[1].Price, productDto2.Price);
            Assert.Equal(fakeRepo.Products[1].Quantity, productDto2.Quantity);
            Assert.Equal(2, fakeRepo.Products.Count);
        }

        [Fact]
        public async void CreateProducts__BothExist_RepoFailsOnce_ShouldOkWithEditedDetails()
        {
            //Arrange
            DefaultSetup(singleDto: false);
            productDto1.Name = "New Name";
            productDto1.Price = 2.99;
            productDto1.Quantity = 5;
            int oldStock = productRepoModel.Quantity;
            int product2Stock = 3;
            fakeRepo.Products.Add(new ProductRepoModel
            {
                ProductId = 2,
                Name = "Anything",
                Quantity = product2Stock,
                Price = 3.99
            });
            fakeRepo.FailureAmount = 1;

            //Act
            var result = await controller.Post(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Product);
            Assert.Equal(fakeRepo.Products[0].ProductId, productDto1.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productDto1.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productDto1.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productDto1.Quantity + oldStock);
            Assert.Equal(fakeRepo.Products[1].ProductId, productDto2.ProductId);
            Assert.Equal(fakeRepo.Products[1].Name, productDto2.Name);
            Assert.Equal(fakeRepo.Products[1].Price, productDto2.Price);
            Assert.Equal(fakeRepo.Products[1].Quantity, productDto2.Quantity + product2Stock);
            Assert.Equal(2, fakeRepo.Products.Count);
        }

        [Fact]
        public async void CreateProducts_ThreeProductsDontExist_RepoFailsTwice_ShouldOkCreatingNewProducts()
        {
            //Arrange
            DefaultSetup(singleDto: false);
            fakeRepo.Products = new List<ProductRepoModel>();
            fakeRepo.FailureAmount = 2;
            products.Add(new ProductDto
            {
                ProductId = 3,
                Name = "Something",
                Quantity = 7,
                Price = 9.99
            });

            //Act
            var result = await controller.Post(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[1].ProductId, productDto1.ProductId);
            Assert.Equal(fakeRepo.Products[1].Name, productDto1.Name);
            Assert.Equal(fakeRepo.Products[1].Price, productDto1.Price);
            Assert.Equal(fakeRepo.Products[1].Quantity, productDto1.Quantity);
            Assert.Equal(fakeRepo.Products[2].ProductId, productDto2.ProductId);
            Assert.Equal(fakeRepo.Products[2].Name, productDto2.Name);
            Assert.Equal(fakeRepo.Products[2].Price, productDto2.Price);
            Assert.Equal(fakeRepo.Products[2].Quantity, productDto2.Quantity);
            Assert.Equal(fakeRepo.Products[0].ProductId, products[2].ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, products[2].Name);
            Assert.Equal(fakeRepo.Products[0].Price, products[2].Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, products[2].Quantity);
            Assert.Equal(3, fakeRepo.Products.Count);
        }

        [Fact]
        public async void CreateProducts_ThreeProductsDontExist_RepoFailsThrice_ShouldNotFound()
        {
            //Arrange
            DefaultSetup(singleDto: false);
            fakeRepo.FailureAmount = 3;
            products.Add(new ProductDto
            {
                ProductId = 3,
                Name = "Something",
                Quantity = 7,
                Price = 9.99
            });

            //Act
            var result = await controller.Post(products);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.Equal(fakeRepo.Products[0].ProductId, productRepoModel.ProductId);
            Assert.Equal(fakeRepo.Products[0].Name, productRepoModel.Name);
            Assert.Equal(fakeRepo.Products[0].Price, productRepoModel.Price);
            Assert.Equal(fakeRepo.Products[0].Quantity, productRepoModel.Quantity);
            Assert.True(1 == fakeRepo.Products.Count);
        }

        [Fact]
        public async void DeleteProduct_ProductDoesntExist_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Product = null;

            //Act
            var result = await controller.Delete(productDto1.ProductId);

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
            var result = await controller.Delete(productDto1.ProductId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(productDto1.ProductId), Times.Once);
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
            var result = await controller.Delete(productDto1.ProductId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.True(fakeRepo.Products.Count == 0);
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
            var result = await controller.Delete(productDto1.ProductId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.ProductExists(productDto1.ProductId), Times.Once);
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
            var result = await controller.Delete(productDto1.ProductId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }
    }
}
