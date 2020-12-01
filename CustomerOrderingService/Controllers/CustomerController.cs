using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CustomerOrderingService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Order.Repository;
using Order.Repository.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CustomerOrderingService.Controllers
{
    [Route("api/[controller]")]
    public class CustomerController : Controller
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public CustomerController(ILogger<OrderController> logger, IOrderRepository orderRepository, IMapper mapper)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet]
        public async Task<IActionResult> Get(int customerId)
        {
            if (await _orderRepository.CustomerExists(customerId))
            {
                var customer = _mapper.Map<CustomerDto>(await _orderRepository.GetCustomer(customerId));
                if (customer != null)
                {
                    return Ok(customer);
                }
            }
            return NotFound();
        }

        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post(CustomerDto customer)
        {
            return await NewOrEditedCustomer(customer);
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(CustomerDto customer)
        {
            return await NewOrEditedCustomer(customer);
        }

        private async Task<IActionResult> NewOrEditedCustomer(CustomerDto customer)
        {
            if (!await _orderRepository.CustomerExists(customer.CustomerId))
            {
                if (await _orderRepository.NewCustomer(_mapper.Map<CustomerEFModel>(customer)))
                {
                    return Ok();
                }
            }
            else
            {
                if (await _orderRepository.EditCustomer(_mapper.Map<CustomerEFModel>(customer)))
                {
                    return Ok();
                }
            }
            return NotFound();
        }

        // DELETE api/<controller>/5
        public async Task<IActionResult> Delete(int customerId)
        {
            if (await AnonymiseCustomer(customerId))
            {
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
                EmailAddress = "Anonymised",
                TelephoneNumber = "Anonymised",
                CanPurchase = false,
                Active = false
            };
            return await _orderRepository.AnonymiseCustomer(_mapper.Map<CustomerEFModel>(customer));
        }
    }
}
