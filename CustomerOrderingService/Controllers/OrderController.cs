using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CustomerOrderingService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Order.Repository;
using Order.Repository.Models;

namespace CustomerOrderingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public OrderController(ILogger<OrderController> logger, IOrderRepository orderRepository, IMapper mapper)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get (int customerId)
        {
            //map coming out
            CustomerDto customer = _mapper.Map<CustomerDto>(await _orderRepository.GetCustomer(customerId));
            List<OrderDto> orders = _mapper.Map<List<OrderDto>>(await _orderRepository.GetCustomerOrders(customerId));
            foreach(OrderDto order in orders)
            {
                order.Products = _mapper.Map<List<OrderedItemDto>>(await _orderRepository.GetOrderItems(order.Id));
            }
            OrderHistoryDto orderHistory = new OrderHistoryDto
            {
                Customer = customer,
                Orders = orders
            };
            return Ok(orderHistory);
        }

        [HttpPost]
        public async Task<IActionResult> Create(FinalisedOrderDto order)
        {
            if (await _orderRepository.CreateOrder(_mapper.Map<FinalisedOrderEFModel>(order)))
            {

                _orderRepository.ClearBasket(order.CustomerId);
            }

        }

        //could also have edit order and cancel order however not in brief, implement later if time
    }
}