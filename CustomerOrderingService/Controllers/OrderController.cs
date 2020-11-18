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
using StaffProduct.Facade;
using StaffProduct.Facade.Models;

namespace CustomerOrderingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IStaffProductFacade _staffProductFacade;
        

        public OrderController(ILogger<OrderController> logger, IOrderRepository orderRepository, IMapper mapper, IStaffProductFacade staffProductFacade)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _mapper = mapper;
            _staffProductFacade = staffProductFacade;
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
            //how is stock reduced in customer product service
            //need to send the order to invoice service
            if (await _orderRepository.CreateOrder(_mapper.Map<FinalisedOrderEFModel>(order)))
            {
                var stockReductionList = new List<StockReductionDto>();
                foreach (OrderedItemDto item in order.OrderedItems)
                {
                    stockReductionList.Add(_mapper.Map<StockReductionDto>(item));
                }
                if(await _staffProductFacade.UpdateStock(stockReductionList))
                {
                    await _orderRepository.ClearBasket(order.CustomerId);
                    return Ok();
                }
            }
            return NotFound();
        }

        //could also have edit order and cancel order however not in brief, implement later if time
    }
}