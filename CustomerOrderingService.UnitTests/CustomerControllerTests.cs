using AspNet.Security.OpenIdConnect.Primitives;
using AutoMapper;
using CustomerAccount.Facade;
using CustomerAccount.Facade.Models;
using CustomerOrderingService.Controllers;
using CustomerOrderingService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Order.Repository;
using Order.Repository.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace CustomerOrderingService.UnitTests
{
    public class CustomerControllerTests
    {
        private CustomerDto customerDto;
        private CustomerRepoModel customerRepoModel;
        private FakeOrderRepository fakeRepo;
        private Mock<IOrderRepository> mockRepo;
        private FakeCustomerFacade fakeCustomerFacade;
        private Mock<ICustomerAccountFacade> mockCustomerFacade;
        private IMapper mapper;
        private ILogger<CustomerController> logger;
        private CustomerController controller;

        private void SetStandardCustomerDto()
        {
            customerDto = new CustomerDto
            {
                CustomerId = 1,
                GivenName = "Fake",
                FamilyName = "Name",
                AddressOne = "Address 1",
                AddressTwo = "Address 2",
                Town = "Town",
                State = "State",
                AreaCode = "Area Code",
                Country = "Country",
                EmailAddress = "email@email.com",
                TelephoneNumber = "07123456789",
                CanPurchase = true,
                Active = true
            };
        }

        private void SetStandardCustomerRepoModel()
        {
            customerRepoModel = new CustomerRepoModel
            {
                CustomerId = 1,
                GivenName = "Fake",
                FamilyName = "Name",
                AddressOne = "Address 1",
                AddressTwo = "Address 2",
                Town = "Town",
                State = "State",
                AreaCode = "Area Code",
                Country = "Country",
                EmailAddress = "email@email.com",
                TelephoneNumber = "07123456789",
                CanPurchase = true,
                Active = true
            };
        }

        private CustomerDto GetEditedDetailsDto()
        {
            return new CustomerDto
            {
                CustomerId = 1,
                GivenName = "NewName",
                FamilyName = "NewName",
                AddressOne = "NewAddress",
                AddressTwo = "NewAddress",
                Town = "NewTown",
                State = "NewState",
                AreaCode = "New Area Code",
                Country = "New Country",
                EmailAddress = "newemail@email.com",
                TelephoneNumber = "07000000000",
                CanPurchase = true,
                Active = true
            };
        }

        private void SetFakeRepo(CustomerRepoModel customer)
        {
            fakeRepo = new FakeOrderRepository
            {
                Customer = customer
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
                .CreateLogger<CustomerController>();
        }

        private void SetMockCustomerRepo(bool customerExists = true, bool customerActive = true, bool succeeds = true, bool authMatch = true)
        {
            mockRepo = new Mock<IOrderRepository>(MockBehavior.Strict);
            mockRepo.Setup(repo => repo.CustomerExists(It.IsAny<int>())).ReturnsAsync(customerExists).Verifiable();
            mockRepo.Setup(repo => repo.IsCustomerActive(It.IsAny<int>())).ReturnsAsync(customerActive).Verifiable();
            mockRepo.Setup(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>())).ReturnsAsync(customerDto.CustomerId).Verifiable();
            mockRepo.Setup(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>())).ReturnsAsync(succeeds).Verifiable();
            mockRepo.Setup(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>())).ReturnsAsync(succeeds).Verifiable();
            if (succeeds)
            {
                mockRepo.Setup(repo => repo.GetCustomer(It.IsAny<int>())).ReturnsAsync(customerRepoModel).Verifiable();
            }
            else
            {
                mockRepo.Setup(repo => repo.GetCustomer(It.IsAny<int>())).ReturnsAsync((CustomerRepoModel)null).Verifiable();
            }
        }

        private void SetMockOrderFacade(bool customerExists = true, bool succeeds = true)
        {
            mockCustomerFacade = new Mock<ICustomerAccountFacade>(MockBehavior.Strict);
            mockCustomerFacade.Setup(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>())).ReturnsAsync(succeeds).Verifiable();
            mockCustomerFacade.Setup(facade => facade.DeleteCustomer(It.IsAny<int>())).ReturnsAsync(succeeds).Verifiable();
        }

        private void SetupUser(CustomerController controller)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, "name"),
                                        new Claim(ClaimTypes.Name, "name"),
                                        new Claim(OpenIdConnectConstants.Claims.Subject, "fakeauthid" ),
                                        new Claim("client_id","customer_web_app")
                                   }, "TestAuth"));
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        }

        private void SetupApi(CustomerController controller)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim("client_id","customer_account_api")
                                   }, "TestAuth"));
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        }

        private void SetFakeFacade()
        {
            fakeCustomerFacade = new FakeCustomerFacade();
        }

        private void DefaultSetup(bool withMocks = false, bool setupUser = true, bool setupApi = false)
        {
            SetStandardCustomerDto();
            SetStandardCustomerRepoModel();
            SetFakeRepo(customerRepoModel);
            SetFakeFacade();
            SetMapper();
            SetLogger();
            SetMockCustomerRepo();
            SetMockOrderFacade();
            if (!withMocks)
            {
                controller = new CustomerController(logger, fakeRepo, mapper, fakeCustomerFacade);
            }
            else
            {
                controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            }
            if (setupUser)
            {
                SetupUser(controller);
            }
            if (setupApi)
            {
                SetupApi(controller);
            }
        }

        [Fact]
        public async void GetExistingCustomer_ShouldOk()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await controller.Get(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var customer = objResult.Value as CustomerDto;
            Assert.NotNull(customer);
            Assert.Equal(customerRepoModel.CustomerId, customer.CustomerId);
            Assert.Equal(customerRepoModel.GivenName, customer.GivenName);
            Assert.Equal(customerRepoModel.FamilyName, customer.FamilyName);
            Assert.Equal(customerRepoModel.AddressOne, customer.AddressOne);
            Assert.Equal(customerRepoModel.AddressTwo, customer.AddressTwo);
            Assert.Equal(customerRepoModel.Town, customer.Town);
            Assert.Equal(customerRepoModel.State, customer.State);
            Assert.Equal(customerRepoModel.AreaCode, customer.AreaCode);
            Assert.Equal(customerRepoModel.Country, customer.Country);
            Assert.Equal(customerRepoModel.EmailAddress, customer.EmailAddress);
            Assert.Equal(customerRepoModel.TelephoneNumber, customer.TelephoneNumber);
            Assert.Equal(customerRepoModel.CanPurchase, customer.CanPurchase);
            Assert.Equal(customerRepoModel.Active, customer.Active);
        }

        [Fact]
        public async void GetExistingCustomer_VerifyMockCalls()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            int customerId = 1;

            //Act
            var result = await controller.Get(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var customer = objResult.Value as CustomerDto;
            Assert.NotNull(customer);
            Assert.Equal(customerRepoModel.CustomerId, customer.CustomerId);
            Assert.Equal(customerRepoModel.GivenName, customer.GivenName);
            Assert.Equal(customerRepoModel.FamilyName, customer.FamilyName);
            Assert.Equal(customerRepoModel.AddressOne, customer.AddressOne);
            Assert.Equal(customerRepoModel.AddressTwo, customer.AddressTwo);
            Assert.Equal(customerRepoModel.Town, customer.Town);
            Assert.Equal(customerRepoModel.State, customer.State);
            Assert.Equal(customerRepoModel.AreaCode, customer.AreaCode);
            Assert.Equal(customerRepoModel.Country, customer.Country);
            Assert.Equal(customerRepoModel.EmailAddress, customer.EmailAddress);
            Assert.Equal(customerRepoModel.TelephoneNumber, customer.TelephoneNumber);
            Assert.Equal(customerRepoModel.CanPurchase, customer.CanPurchase);
            Assert.Equal(customerRepoModel.Active, customer.Active);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(customerId), Times.Once);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void GetCustomer_DoesntExist_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await controller.Get(2);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void GetCustomer_DoesntExist_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(customerExists: false, customerActive: true, succeeds: true);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            int customerId = 2;

            //Act
            var result = await controller.Get(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void GetCustomer_NotActive_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            customerRepoModel.Active = false;

            //Act
            var result = await controller.Get(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void GetCustomer_NotActive_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(customerExists: true, customerActive: false, succeeds: true);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            int customerId = 1;

            //Act
            var result = await controller.Get(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }


        [Fact]
        public async void GetCustomer_RepoFailure_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(succeeds: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            SetupUser(controller);
            int customerId = 1;

            //Act
            var result = await controller.Get(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(customerId), Times.Once);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void GetCustomer_NoUser_ShouldForbid()
        {
            //Arrange
            DefaultSetup(setupUser: false);

            //Act
            var result = await controller.Get(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as ForbidResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void EditCustomer_CustomerDoesntExist_ShouldOkCreatingNewCustomer()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer = null;

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void EditCustomer_CustomerDoesntExist_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(customerExists: false, customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void EditCustomer__ShouldOkWithEditedDetails()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerDto.CustomerId);
            Assert.Equal(fakeRepo.Customer.GivenName, customerDto.GivenName);
            Assert.Equal(fakeRepo.Customer.FamilyName, customerDto.FamilyName);
            Assert.Equal(fakeRepo.Customer.AddressOne, customerDto.AddressOne);
            Assert.Equal(fakeRepo.Customer.AddressTwo, customerDto.AddressTwo);
            Assert.Equal(fakeRepo.Customer.Town, customerDto.Town);
            Assert.Equal(fakeRepo.Customer.State, customerDto.State);
            Assert.Equal(fakeRepo.Customer.AreaCode, customerDto.AreaCode);
            Assert.Equal(fakeRepo.Customer.Country, customerDto.Country);
            Assert.Equal(fakeRepo.Customer.EmailAddress, customerDto.EmailAddress);
            Assert.Equal(fakeRepo.Customer.TelephoneNumber, customerDto.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.CanPurchase, customerDto.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, customerDto.Active);
        }

        [Fact]
        public async void EditCustomer_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo();
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Once);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void EditCustomer_CustomerIsInactive_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.Active = false;
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, editedCustomer.CustomerId);
            Assert.NotEqual(fakeRepo.Customer.GivenName, editedCustomer.GivenName);
            Assert.NotEqual(fakeRepo.Customer.FamilyName, editedCustomer.FamilyName);
            Assert.NotEqual(fakeRepo.Customer.AddressOne, editedCustomer.AddressOne);
            Assert.NotEqual(fakeRepo.Customer.AddressTwo, editedCustomer.AddressTwo);
            Assert.NotEqual(fakeRepo.Customer.Town, editedCustomer.Town);
            Assert.NotEqual(fakeRepo.Customer.State, editedCustomer.State);
            Assert.NotEqual(fakeRepo.Customer.AreaCode, editedCustomer.AreaCode);
            Assert.NotEqual(fakeRepo.Customer.Country, editedCustomer.Country);
            Assert.NotEqual(fakeRepo.Customer.EmailAddress, editedCustomer.EmailAddress);
            Assert.NotEqual(fakeRepo.Customer.TelephoneNumber, editedCustomer.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.CanPurchase, editedCustomer.CanPurchase);
            Assert.NotEqual(fakeRepo.Customer.Active, editedCustomer.Active);
        }

        [Fact]
        public async void EditCustomer_CustomerIsInactive_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            SetupUser(controller);
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void EditCustomer_NullCustomer_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await controller.Put(1, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void EditCustomer_NullCustomer_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(authMatch: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            SetupUser(controller);
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Put(1, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }


        [Fact]
        public async void DeleteExistingCustomer_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            int customerId = 1;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal("Anonymised", fakeRepo.Customer.GivenName);
            Assert.Equal("Anonymised", fakeRepo.Customer.FamilyName);
            Assert.Equal("Anonymised", fakeRepo.Customer.AddressOne);
            Assert.Equal("Anonymised", fakeRepo.Customer.AddressTwo);
            Assert.Equal("Anonymised", fakeRepo.Customer.Town);
            Assert.Equal("Anonymised", fakeRepo.Customer.State);
            Assert.Equal("Anonymised", fakeRepo.Customer.AreaCode);
            Assert.Equal("Anonymised", fakeRepo.Customer.Country);
            Assert.Equal("anon@anon.com", fakeRepo.Customer.EmailAddress);
            Assert.Equal("00000000000", fakeRepo.Customer.TelephoneNumber);
            Assert.True(false == fakeRepo.Customer.CanPurchase);
            Assert.True(false == fakeRepo.Customer.Active);
        }

        [Fact]
        public async void DeleteExistingCustomer_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            int customerId = 1;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(customerDto.CustomerId), Times.Once);
        }

        [Fact]
        public async void DeleteCustomer_CustomerDoesntExist_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            int customerId = 2;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.Equal(customerRepoModel.GivenName, fakeRepo.Customer.GivenName);
            Assert.Equal(customerRepoModel.FamilyName, fakeRepo.Customer.FamilyName);
            Assert.Equal(customerRepoModel.AddressOne, fakeRepo.Customer.AddressOne);
            Assert.Equal(customerRepoModel.AddressTwo, fakeRepo.Customer.AddressTwo);
            Assert.Equal(customerRepoModel.Town, fakeRepo.Customer.Town);
            Assert.Equal(customerRepoModel.State, fakeRepo.Customer.State);
            Assert.Equal(customerRepoModel.AreaCode, fakeRepo.Customer.AreaCode);
            Assert.Equal(customerRepoModel.Country, fakeRepo.Customer.Country);
            Assert.Equal(customerRepoModel.EmailAddress, fakeRepo.Customer.EmailAddress);
            Assert.Equal(customerRepoModel.TelephoneNumber, fakeRepo.Customer.TelephoneNumber);
            Assert.True(customerRepoModel.CanPurchase == fakeRepo.Customer.CanPurchase);
            Assert.True(customerRepoModel.Active == fakeRepo.Customer.Active);
        }

        [Fact]
        public async void DeleteExistingCustomer_CustomerDoesntExist_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(customerExists: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            SetupUser(controller);
            int customerId = 2;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void DeleteExistingCustomer_CustomerNotActive_ShouldOk()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.Active = false;

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal("Anonymised", fakeRepo.Customer.GivenName);
            Assert.Equal("Anonymised", fakeRepo.Customer.FamilyName);
            Assert.Equal("Anonymised", fakeRepo.Customer.AddressOne);
            Assert.Equal("Anonymised", fakeRepo.Customer.AddressTwo);
            Assert.Equal("Anonymised", fakeRepo.Customer.Town);
            Assert.Equal("Anonymised", fakeRepo.Customer.State);
            Assert.Equal("Anonymised", fakeRepo.Customer.AreaCode);
            Assert.Equal("Anonymised", fakeRepo.Customer.Country);
            Assert.Equal("anon@anon.com", fakeRepo.Customer.EmailAddress);
            Assert.Equal("00000000000", fakeRepo.Customer.TelephoneNumber);
            Assert.True(false == fakeRepo.Customer.CanPurchase);
            Assert.True(false == fakeRepo.Customer.Active);
        }

        [Fact]
        public async void DeleteExistingCustomer_CustomerNotActive_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(customerDto.CustomerId), Times.Once);
        }

        [Fact]
        public async void DeleteExistingCustomer_RepoFails_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.AutoFails = true;
            int customerId = 1;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.Equal(customerRepoModel.GivenName, fakeRepo.Customer.GivenName);
            Assert.Equal(customerRepoModel.FamilyName, fakeRepo.Customer.FamilyName);
            Assert.Equal(customerRepoModel.AddressOne, fakeRepo.Customer.AddressOne);
            Assert.Equal(customerRepoModel.AddressTwo, fakeRepo.Customer.AddressTwo);
            Assert.Equal(customerRepoModel.Town, fakeRepo.Customer.Town);
            Assert.Equal(customerRepoModel.State, fakeRepo.Customer.State);
            Assert.Equal(customerRepoModel.AreaCode, fakeRepo.Customer.AreaCode);
            Assert.Equal(customerRepoModel.Country, fakeRepo.Customer.Country);
            Assert.Equal(customerRepoModel.EmailAddress, fakeRepo.Customer.EmailAddress);
            Assert.Equal(customerRepoModel.TelephoneNumber, fakeRepo.Customer.TelephoneNumber);
            Assert.True(customerRepoModel.CanPurchase == fakeRepo.Customer.CanPurchase);
            Assert.True(customerRepoModel.Active == fakeRepo.Customer.Active);
        }

        [Fact]
        public async void GetExistingCustomer_FromOrderingApi_ShouldOk()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);

            //Act
            var result = await controller.Get(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var customer = objResult.Value as CustomerDto;
            Assert.NotNull(customer);
            Assert.Equal(customerRepoModel.CustomerId, customer.CustomerId);
            Assert.Equal(customerRepoModel.GivenName, customer.GivenName);
            Assert.Equal(customerRepoModel.FamilyName, customer.FamilyName);
            Assert.Equal(customerRepoModel.AddressOne, customer.AddressOne);
            Assert.Equal(customerRepoModel.AddressTwo, customer.AddressTwo);
            Assert.Equal(customerRepoModel.Town, customer.Town);
            Assert.Equal(customerRepoModel.State, customer.State);
            Assert.Equal(customerRepoModel.AreaCode, customer.AreaCode);
            Assert.Equal(customerRepoModel.Country, customer.Country);
            Assert.Equal(customerRepoModel.EmailAddress, customer.EmailAddress);
            Assert.Equal(customerRepoModel.TelephoneNumber, customer.TelephoneNumber);
            Assert.Equal(customerRepoModel.CanPurchase, customer.CanPurchase);
            Assert.Equal(customerRepoModel.Active, customer.Active);
        }

        [Fact]
        public async void GetExistingCustomer_FromOrderingApi_VerifyMockCalls()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true, withMocks: true);
            int customerId = 1;

            //Act
            var result = await controller.Get(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkObjectResult;
            Assert.NotNull(objResult);
            var customer = objResult.Value as CustomerDto;
            Assert.NotNull(customer);
            Assert.Equal(customerRepoModel.CustomerId, customer.CustomerId);
            Assert.Equal(customerRepoModel.GivenName, customer.GivenName);
            Assert.Equal(customerRepoModel.FamilyName, customer.FamilyName);
            Assert.Equal(customerRepoModel.AddressOne, customer.AddressOne);
            Assert.Equal(customerRepoModel.AddressTwo, customer.AddressTwo);
            Assert.Equal(customerRepoModel.Town, customer.Town);
            Assert.Equal(customerRepoModel.State, customer.State);
            Assert.Equal(customerRepoModel.AreaCode, customer.AreaCode);
            Assert.Equal(customerRepoModel.Country, customer.Country);
            Assert.Equal(customerRepoModel.EmailAddress, customer.EmailAddress);
            Assert.Equal(customerRepoModel.TelephoneNumber, customer.TelephoneNumber);
            Assert.Equal(customerRepoModel.CanPurchase, customer.CanPurchase);
            Assert.Equal(customerRepoModel.Active, customer.Active);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(customerDto.CustomerId), Times.Once);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void GetCustomer_DoesntExist_FromCustomerApi_ShouldNotFound()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);

            //Act
            var result = await controller.Get(2);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void GetCustomer_DoesntExist_FromCustomerApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true, withMocks: true);
            SetMockCustomerRepo(customerExists: false, customerActive: true, succeeds: true);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            int customerId = 2;

            //Act
            var result = await controller.Get(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void GetCustomer_NotActive_FromCustomerApi_ShouldNotFound()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);
            customerRepoModel.Active = false;

            //Act
            var result = await controller.Get(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void GetCustomer_NotActive_FromCustomerApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true, withMocks: true);
            SetMockCustomerRepo(customerExists: true, customerActive: false, succeeds: true);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            int customerId = 1;

            //Act
            var result = await controller.Get(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }


            [Fact]
        public async void GetCustomer_RepoFailure_FromCustomerApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true, withMocks: true);
            SetMockCustomerRepo(succeeds: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            SetupUser(controller);
            int customerId = 1;

            //Act
            var result = await controller.Get(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(customerDto.CustomerId), Times.Once);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void EditCustomer_CustomerDoesntExist_FromCustomerApi_ShouldNotFound()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);
            fakeRepo.Customer = null;

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void EditCustomer_CustomerDoesntExist_FromCustomerApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            SetMockCustomerRepo(customerExists: false, customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void EditCustomer__FromCustomerApi_ShouldOkWithEditedDetails()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerDto.CustomerId);
            Assert.Equal(fakeRepo.Customer.GivenName, customerDto.GivenName);
            Assert.Equal(fakeRepo.Customer.FamilyName, customerDto.FamilyName);
            Assert.Equal(fakeRepo.Customer.AddressOne, customerDto.AddressOne);
            Assert.Equal(fakeRepo.Customer.AddressTwo, customerDto.AddressTwo);
            Assert.Equal(fakeRepo.Customer.Town, customerDto.Town);
            Assert.Equal(fakeRepo.Customer.State, customerDto.State);
            Assert.Equal(fakeRepo.Customer.AreaCode, customerDto.AreaCode);
            Assert.Equal(fakeRepo.Customer.Country, customerDto.Country);
            Assert.Equal(fakeRepo.Customer.EmailAddress, customerDto.EmailAddress);
            Assert.Equal(fakeRepo.Customer.TelephoneNumber, customerDto.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.CanPurchase, customerDto.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, customerDto.Active);
        }

        [Fact]
        public async void EditCustomer_FromCustomerApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            SetMockCustomerRepo();
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Once);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void EditCustomer_CustomerIsInactive_FromCustomerApi_ShouldNotFound()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);
            fakeRepo.Customer.Active = false;
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, editedCustomer.CustomerId);
            Assert.NotEqual(fakeRepo.Customer.GivenName, editedCustomer.GivenName);
            Assert.NotEqual(fakeRepo.Customer.FamilyName, editedCustomer.FamilyName);
            Assert.NotEqual(fakeRepo.Customer.AddressOne, editedCustomer.AddressOne);
            Assert.NotEqual(fakeRepo.Customer.AddressTwo, editedCustomer.AddressTwo);
            Assert.NotEqual(fakeRepo.Customer.Town, editedCustomer.Town);
            Assert.NotEqual(fakeRepo.Customer.State, editedCustomer.State);
            Assert.NotEqual(fakeRepo.Customer.AreaCode, editedCustomer.AreaCode);
            Assert.NotEqual(fakeRepo.Customer.Country, editedCustomer.Country);
            Assert.NotEqual(fakeRepo.Customer.EmailAddress, editedCustomer.EmailAddress);
            Assert.NotEqual(fakeRepo.Customer.TelephoneNumber, editedCustomer.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.CanPurchase, editedCustomer.CanPurchase);
            Assert.NotEqual(fakeRepo.Customer.Active, editedCustomer.Active);
        }

        [Fact]
        public async void EditCustomer_CustomerIsInactive_FromCustomerApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            SetMockCustomerRepo(customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            SetupUser(controller);
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Put(customerDto.CustomerId, customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void EditCustomer_NullCustomer_FromCustomerApi_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);

            //Act
            var result = await controller.Put(1, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void EditCustomer_NullCustomer_FFromCustomerApi_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            SetMockCustomerRepo(authMatch: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            SetupUser(controller);
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Put(1, null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void DeleteExistingCustomer_FromCustomerApi_ShouldOk()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);
            int customerId = 1;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal("Anonymised", fakeRepo.Customer.GivenName);
            Assert.Equal("Anonymised", fakeRepo.Customer.FamilyName);
            Assert.Equal("Anonymised", fakeRepo.Customer.AddressOne);
            Assert.Equal("Anonymised", fakeRepo.Customer.AddressTwo);
            Assert.Equal("Anonymised", fakeRepo.Customer.Town);
            Assert.Equal("Anonymised", fakeRepo.Customer.State);
            Assert.Equal("Anonymised", fakeRepo.Customer.AreaCode);
            Assert.Equal("Anonymised", fakeRepo.Customer.Country);
            Assert.Equal("anon@anon.com", fakeRepo.Customer.EmailAddress);
            Assert.Equal("00000000000", fakeRepo.Customer.TelephoneNumber);
            Assert.True(false == fakeRepo.Customer.CanPurchase);
            Assert.True(false == fakeRepo.Customer.Active);
        }

        [Fact]
        public async void DeleteExistingCustomer_FromCustomerApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            int customerId = 1;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void DeleteCustomer_CustomerDoesntExist_FromCustomerApi_ShouldNotFound()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);
            int customerId = 2;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.Equal(customerRepoModel.GivenName, fakeRepo.Customer.GivenName);
            Assert.Equal(customerRepoModel.FamilyName, fakeRepo.Customer.FamilyName);
            Assert.Equal(customerRepoModel.AddressOne, fakeRepo.Customer.AddressOne);
            Assert.Equal(customerRepoModel.AddressTwo, fakeRepo.Customer.AddressTwo);
            Assert.Equal(customerRepoModel.Town, fakeRepo.Customer.Town);
            Assert.Equal(customerRepoModel.State, fakeRepo.Customer.State);
            Assert.Equal(customerRepoModel.AreaCode, fakeRepo.Customer.AreaCode);
            Assert.Equal(customerRepoModel.Country, fakeRepo.Customer.Country);
            Assert.Equal(customerRepoModel.EmailAddress, fakeRepo.Customer.EmailAddress);
            Assert.Equal(customerRepoModel.TelephoneNumber, fakeRepo.Customer.TelephoneNumber);
            Assert.True(customerRepoModel.CanPurchase == fakeRepo.Customer.CanPurchase);
            Assert.True(customerRepoModel.Active == fakeRepo.Customer.Active);
        }

        [Fact]
        public async void DeleteExistingCustomer_CustomerDoesntExist_FromCustomerApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            SetMockCustomerRepo(customerExists: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            SetupUser(controller);
            int customerId = 2;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void DeleteExistingCustomer_CustomerNotActive_FromCustomerApi_ShouldOk()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);
            fakeRepo.Customer.Active = false;

            //Act
            var result = await controller.Delete(1);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.Equal("Anonymised", fakeRepo.Customer.GivenName);
            Assert.Equal("Anonymised", fakeRepo.Customer.FamilyName);
            Assert.Equal("Anonymised", fakeRepo.Customer.AddressOne);
            Assert.Equal("Anonymised", fakeRepo.Customer.AddressTwo);
            Assert.Equal("Anonymised", fakeRepo.Customer.Town);
            Assert.Equal("Anonymised", fakeRepo.Customer.State);
            Assert.Equal("Anonymised", fakeRepo.Customer.AreaCode);
            Assert.Equal("Anonymised", fakeRepo.Customer.Country);
            Assert.Equal("anon@anon.com", fakeRepo.Customer.EmailAddress);
            Assert.Equal("00000000000", fakeRepo.Customer.TelephoneNumber);
            Assert.True(false == fakeRepo.Customer.CanPurchase);
            Assert.True(false == fakeRepo.Customer.Active);
        }

        [Fact]
        public async void DeleteExistingCustomer_CustomerNotActive_FromCustomerApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            SetMockCustomerRepo(customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            SetupUser(controller);
            int customerId = 1;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(customerId), Times.Once);
        }

        [Fact]
        public async void DeleteExistingCustomer_RepoFails_FromCustomerApi_ShouldNotFound()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);
            fakeRepo.AutoFails = true;
            int customerId = 1;

            //Act
            var result = await controller.Delete(customerId);

            //Assert
            Assert.NotNull(result);
            var objResult = result as NotFoundResult;
            Assert.NotNull(objResult);
            Assert.Equal(customerRepoModel.GivenName, fakeRepo.Customer.GivenName);
            Assert.Equal(customerRepoModel.FamilyName, fakeRepo.Customer.FamilyName);
            Assert.Equal(customerRepoModel.AddressOne, fakeRepo.Customer.AddressOne);
            Assert.Equal(customerRepoModel.AddressTwo, fakeRepo.Customer.AddressTwo);
            Assert.Equal(customerRepoModel.Town, fakeRepo.Customer.Town);
            Assert.Equal(customerRepoModel.State, fakeRepo.Customer.State);
            Assert.Equal(customerRepoModel.AreaCode, fakeRepo.Customer.AreaCode);
            Assert.Equal(customerRepoModel.Country, fakeRepo.Customer.Country);
            Assert.Equal(customerRepoModel.EmailAddress, fakeRepo.Customer.EmailAddress);
            Assert.Equal(customerRepoModel.TelephoneNumber, fakeRepo.Customer.TelephoneNumber);
            Assert.True(customerRepoModel.CanPurchase == fakeRepo.Customer.CanPurchase);
            Assert.True(customerRepoModel.Active == fakeRepo.Customer.Active);
        }

        [Fact]
        public async void PostNewCustomer_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(customerExists: false, customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostNewCustomer_CustomerAlreadyExists_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup();

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostNewCustomer_CustomerAlreadyExists_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo();
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            SetupUser(controller);

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostNewCustomer_CustomerAlreadyExistsAndInactive_ShouldNotFound()
        {
            //Arrange
            DefaultSetup();
            fakeRepo.Customer.Active = false;
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Post(editedCustomer);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostNewCustomer_CustomerAlreadyExistsAndInactive_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            SetupUser(controller);
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Post(editedCustomer);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostNewCustomer_NullCustomer_ShouldUnprocessableEntity()
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
        public async void PostNewCustomer_NullCustomer_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true);
            SetMockCustomerRepo(authMatch: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            SetupUser(controller);
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Post(null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostNewCustomer_FromCustomerApi_ShouldOk()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);
            fakeRepo.Customer = null;

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            Assert.NotNull(fakeRepo.Customer);
            Assert.Equal(fakeRepo.Customer.CustomerId, customerDto.CustomerId);
            Assert.Equal(fakeRepo.Customer.GivenName, customerDto.GivenName);
            Assert.Equal(fakeRepo.Customer.FamilyName, customerDto.FamilyName);
            Assert.Equal(fakeRepo.Customer.AddressOne, customerDto.AddressOne);
            Assert.Equal(fakeRepo.Customer.AddressTwo, customerDto.AddressTwo);
            Assert.Equal(fakeRepo.Customer.Town, customerDto.Town);
            Assert.Equal(fakeRepo.Customer.State, customerDto.State);
            Assert.Equal(fakeRepo.Customer.AreaCode, customerDto.AreaCode);
            Assert.Equal(fakeRepo.Customer.Country, customerDto.Country);
            Assert.Equal(fakeRepo.Customer.EmailAddress, customerDto.EmailAddress);
            Assert.Equal(fakeRepo.Customer.TelephoneNumber, customerDto.TelephoneNumber);
            Assert.Equal(fakeRepo.Customer.CanPurchase, customerDto.CanPurchase);
            Assert.Equal(fakeRepo.Customer.Active, customerDto.Active);
        }

        [Fact]
        public async void PostNewCustomer_FromCustomerApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            SetMockCustomerRepo(customerExists: false, customerActive: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            SetupApi(controller);

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as OkResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Once);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostNewCustomer_CustomerAlreadyExists_FromCustomerApi_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostNewCustomer_CustomerAlreadyExists_FromCustomerApi_VerifyMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            SetMockCustomerRepo();
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            SetupApi(controller);

            //Act
            var result = await controller.Post(customerDto);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(customerDto.CustomerId), Times.Once);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void PostNewCustomer_NullCustomer_FromCustomerApi_ShouldUnprocessableEntity()
        {
            //Arrange
            DefaultSetup(setupUser: false, setupApi: true);

            //Act
            var result = await controller.Post(null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
        }

        [Fact]
        public async void PostNewCustomer_NullCustomer_FromCustomerApi_CheckMocks()
        {
            //Arrange
            DefaultSetup(withMocks: true, setupUser: false, setupApi: true);
            SetMockCustomerRepo(authMatch: false);
            controller = new CustomerController(logger, mockRepo.Object, mapper, mockCustomerFacade.Object);
            SetupUser(controller);
            var editedCustomer = GetEditedDetailsDto();

            //Act
            var result = await controller.Post(null);

            //Assert
            Assert.NotNull(result);
            var objResult = result as UnprocessableEntityResult;
            Assert.NotNull(objResult);
            mockRepo.Verify(repo => repo.CustomerExists(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.IsCustomerActive(It.IsAny<int>()), Times.Never);
            mockRepo.Verify(repo => repo.NewCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.EditCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.AnonymiseCustomer(It.IsAny<CustomerRepoModel>()), Times.Never);
            mockRepo.Verify(repo => repo.GetCustomer(It.IsAny<int>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.EditCustomer(It.IsAny<CustomerFacadeDto>()), Times.Never);
            mockCustomerFacade.Verify(facade => facade.DeleteCustomer(It.IsAny<int>()), Times.Never);
        }
    }
}
