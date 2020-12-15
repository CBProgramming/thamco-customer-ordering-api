using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CustomerOrderingService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Order.Repository;
using Order.Repository.Models;

namespace CustomerOrderingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "CustomerOnly")]
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
        [HttpGet("{customerId}")]
        public async Task<IActionResult> Get([FromRoute] int customerId)
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
        public async Task<IActionResult> Create([FromBody] BasketItemDto newItem)
        {
            return await CreateOrEditBasketItem(newItem);
        }

        //Edit product in basket
        [HttpPut]
        public async Task<IActionResult> Edit([FromRoute] int customerId, [FromBody] BasketItemDto editedItem)
        {
            return await CreateOrEditBasketItem(editedItem);
        }

        private async Task<IActionResult> CreateOrEditBasketItem(BasketItemDto basketItem)
        {
            if (await _orderRepository.CustomerExists(basketItem.CustomerId))
            {
                if (await _orderRepository.IsCustomerActive(basketItem.CustomerId))
                {
                    bool itemIsInBasket = await _orderRepository.IsItemInBasket(basketItem.CustomerId, basketItem.ProductId);
                    if (itemIsInBasket && basketItem.Quantity == 0)
                    {
                        return await Delete(basketItem.CustomerId, basketItem.ProductId);
                    }
                    if (await IsBasketItemValid(basketItem))
                    {
                        if (await _orderRepository.ProductExists(basketItem.ProductId))
                        {
                            if (itemIsInBasket)
                            {
                                if (await _orderRepository.EditBasketItem(_mapper.Map<BasketItemRepoModel>(basketItem)))
                                {
                                    return Ok();
                                }
                            }
                            else
                            {
                                if (await _orderRepository.AddBasketItem(_mapper.Map<BasketItemRepoModel>(basketItem)))
                                {
                                    return Ok();
                                }

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

        //Remove item from basket
        [HttpDelete]
        public async Task<IActionResult> Delete(int customerId, int productId)
        {
            if (await _orderRepository.CustomerExists(customerId))
            {
                if (await _orderRepository.IsCustomerActive(customerId))
                {
                    if (await _orderRepository.IsItemInBasket(customerId, productId))
                    {
                        return await DeleteBasketItem(customerId, productId);
                    }
                    return NotFound();
                }
                return Forbid();
            }
            return NotFound();
        }

        private async Task<IActionResult> DeleteBasketItem(int customerId, int productId)
        {
            if (await _orderRepository.DeleteBasketItem(customerId, productId))
            {
                return Ok();
            }
            return NotFound();
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
