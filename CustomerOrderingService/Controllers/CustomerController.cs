﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CustomerAccount.Facade;
using CustomerAccount.Facade.Models;
using CustomerOrderingService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Order.Repository;
using Order.Repository.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CustomerOrderingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "CustomerOrAccountAPI")]
    public class CustomerController : ControllerBase
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly ICustomerAccountFacade _customerFacade;
        private string clientId;

        public CustomerController(ILogger<CustomerController> logger, IOrderRepository orderRepository, 
            IMapper mapper, ICustomerAccountFacade customerFacade)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _mapper = mapper;
            _customerFacade = customerFacade;
        }

        // GET: api/<controller>
        [HttpGet("{customerId}")]
        //[Authorize]
        public async Task<IActionResult> Get([FromRoute] int customerId)
        {
            if (await _orderRepository.CustomerExists(customerId)
                && await _orderRepository.IsCustomerActive(customerId))
            {
                var customer = _mapper.Map<CustomerDto>(await _orderRepository.GetCustomer(customerId));
                if (customer != null)
                {
                    if (User != null && User.Claims != null)
                    {
                        return Ok(customer);
                    }
                    return Forbid();
                }
            }
            return NotFound();
        }

        // PUT api/<controller>/5
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerDto customer)
        {
            getTokenDetails();
            if (customer != null 
                && clientId == "customer_account_api" 
                && ! await _orderRepository.CustomerExists(customer.CustomerId))
            {
                customer.Active = true;
                if (await _orderRepository.NewCustomer(_mapper.Map<CustomerRepoModel>(customer)))
                {
                    return Ok();
                }
            }
            return UnprocessableEntity();
        }

        // PUT api/<controller>/5
        [HttpPut("{customerId}")]
        public async Task<IActionResult> Put([FromRoute] int customerId, [FromBody] CustomerDto customer)
        {
            if (customer != null)
            {
                customer.CustomerId = customerId;
                customer.Active = true;
                if (await _orderRepository.CustomerExists(customerId) && (await _orderRepository.IsCustomerActive(customerId)))
                {
                    if (await _orderRepository.EditCustomer(_mapper.Map<CustomerRepoModel>(customer)))
                    {
                        getTokenDetails();
                        if (clientId != "customer_account_api")
                        {
                            if (!await _customerFacade.EditCustomer(_mapper.Map<CustomerFacadeDto>(customer))) ;
                            {
                                //write to local db to be reattempted later
                            }
                        }
                        return Ok();
                    }
                }
                return NotFound();
            }
            return UnprocessableEntity();
        }


        // DELETE api/<controller>/5
        [HttpDelete("{customerId}")]
        public async Task<IActionResult> Delete([FromRoute] int customerId)
        {
            if (await _orderRepository.CustomerExists(customerId)
                    && await AnonymiseCustomer(customerId))
            {
                getTokenDetails();
                if (clientId != "customer_account_api")
                {
                    if (!await _customerFacade.DeleteCustomer(customerId))
                    {
                        //write to local db to be reattempted later
                    }
                }
                return Ok();
            }
            return NotFound();
        }

        private async Task<bool> AnonymiseCustomer(int customerId)
        {
            var customer = new CustomerDto
            {
                CustomerId = customerId,
                GivenName = "Anonymised",
                FamilyName = "Anonymised",
                AddressOne = "Anonymised",
                AddressTwo = "Anonymised",
                Town = "Anonymised",
                State = "Anonymised",
                AreaCode = "Anonymised",
                Country = "Anonymised",
                EmailAddress = "anon@anon.com",
                TelephoneNumber = "00000000000",
                CanPurchase = false,
                Active = false
            };
            return await _orderRepository.AnonymiseCustomer(_mapper.Map<CustomerRepoModel>(customer));
        }

        private void getTokenDetails()
        {
            clientId = User
                .Claims
                .FirstOrDefault(c => c.Type == "client_id")?.Value;
        }
    }
}
