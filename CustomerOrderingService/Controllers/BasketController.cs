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

namespace CustomerOrderingService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly ILogger<BasketController> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public BasketController(ILogger<BasketController> logger, IOrderRepository orderRepository, IMapper mapper)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        //Get basket
        [HttpGet]
        public async Task<IActionResult> Get(int customerId)
        {
            if (await _orderRepository.CustomerExists(customerId))
            {
                if (await _orderRepository.IsCustomerActive(customerId))
                {
                    List<BasketItemDto> basket = _mapper.Map<List<BasketItemDto>>(await _orderRepository.GetBasket(customerId));
                    return Ok(basket);
                }
                return Forbid();
            }
            return NotFound();
        }

        //Add item to basket
        [HttpPost]
        public async Task<IActionResult> Create(BasketItemDto newItem)
        {
            if (await _orderRepository.CustomerExists(newItem.CustomerId))
            {
                if (await _orderRepository.IsCustomerActive(newItem.CustomerId))
                {
                    if (await IsBasketItemValid(newItem))
                    {
                        if (await _orderRepository.ProductExists(newItem.ProductId))
                        {
                            if (await _orderRepository.AddBasketItem(_mapper.Map<BasketItemEFModel>(newItem)))
                            {
                                return Ok();
                            }
                            return NotFound();
                        }
                        return NotFound();
                    }
                    return UnprocessableEntity();
                }
                return Forbid();
            }
            return NotFound();
            
            
        }

        //Edit product in basket
        [HttpPut]
        public async Task<IActionResult> Edit(BasketItemDto editedItem)
        {
            if (await _orderRepository.EditBasketItem(_mapper.Map<BasketItemEFModel>(editedItem)))
            {
                return Ok();
            }
            return NotFound();
        }

        //Remove item from basket
        [HttpDelete]
        public Task<IActionResult> Delete(int customerId, int productId)
        {
            throw new NotImplementedException();
        }

        private async Task<bool> IsBasketItemValid(BasketItemDto newItem)
        {
            if (newItem != null
                && NameIsValid(newItem.ProductName)
                && PriceIsValid(newItem.Price)
                && QuantityIsValid(newItem.Quantity))
            {
                return true;
            }
            return false;
        }

        private bool QuantityIsValid(int quantity)
        {
            return quantity > 0;
        }

        private bool PriceIsValid(double price)
        {
            return price >= 0;
        }

        private bool NameIsValid(string productName)
        {
            return !string.IsNullOrEmpty(productName);
        }
    }
}
