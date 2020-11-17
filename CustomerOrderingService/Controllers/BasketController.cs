using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CustomerOrderingService.Data;
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
            var basket = _orderRepository.GetBasket(customerId);
            return Ok(basket);
        }

        //Add item to basket
        [HttpPost]
        public async Task<IActionResult> Create(BasketItemDto newItem)
        {
            if (await _orderRepository.AddBasketItem(_mapper.Map<BasketItemModel>(newItem)))
            {
                return Ok();
            }
            return NotFound();
        }

        //Edit product in basket
        [HttpPut]
        public async Task<IActionResult> Edit(BasketItemDto editedItem)
        {
            if (await _orderRepository.EditBasketItem(_mapper.Map<BasketItemModel>(editedItem)))
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
    }
}
