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
using Order.Repository.Data;
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
        public async Task<IActionResult> Get (int customerId, [FromQuery] int? orderId)
        {
            CustomerDto customer = _mapper.Map<CustomerDto>(await _orderRepository.GetCustomer(customerId));
            if (customer != null)
            {
                //get list of orders
                if (orderId == null)
                {
                    List<OrderHistoryDto> orders = _mapper.Map<List<OrderHistoryDto>>(await _orderRepository.GetCustomerOrders(customerId));
                    orders.ForEach(o => o.CustomerId = customerId);
                    return Ok(orders);
                }
                //get specific order
                else
                {
                    OrderDto order = _mapper.Map<OrderDto>(await _orderRepository.GetCustomerOrder(orderId));
                    order.Products = _mapper.Map<List<OrderedItemDto>>(await _orderRepository.GetOrderItems(orderId));
                    return Ok(order);
                }
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create(FinalisedOrderDto order)
        {
            //how is stock reduced in customer product service
            //need to send the order to invoice service
            if(ModelState.IsValid && ValidateOrderedItems(order.OrderedItems))
            {
                //check if customer and products exist
                if (_orderRepository.CustomerExists(order.CustomerId)
                && _orderRepository.ProductsExist(_mapper.Map<List<ProductEFModel>>(order.OrderedItems)))
                {
                    if (_orderRepository.ProductsInStock(_mapper.Map<List<ProductEFModel>>(order.OrderedItems)))
                    {
                        //reduce stock before creating order (it's worse customer service to allow a customer to order something out of stock
                        //than for the company to innacurately display stock levels as lower than they are if an order fails
                        var stockReductionList = new List<StockReductionDto>();
                        foreach (OrderedItemDto item in order.OrderedItems)
                        {
                            stockReductionList.Add(_mapper.Map<StockReductionDto>(item));
                        }
                        if (await _staffProductFacade.UpdateStock(stockReductionList))
                        {
                            //if date is over 7 days old, or a future date, set date to now (7 days chosen arbitrarily as it would be a business decision above my position)
                            if (DateTime.Now.Ticks - order.Date.Ticks > (TimeSpan.TicksPerDay * 7) || order.Date > DateTime.Now)
                            {
                                order.Date = DateTime.Now;
                            }
                            if (await _orderRepository.CreateOrder(_mapper.Map<FinalisedOrderEFModel>(order)))
                            {
                                await _orderRepository.ClearBasket(order.CustomerId);
                                //return ok regardless of if the basket successfully clears because the order is complete
                                //better customer service than clearing basket only to have order fail and customer needs to re-add everything to basket
                                return Ok();
                            }
                        }
                    }
                    //if the item is no longer in stock between placing the order and it going through (ie someone else ordered the last product at the same time)
                    return Conflict();
                }
                return NotFound();
            }
            return UnprocessableEntity();
            
        }

        private bool ValidateOrderedItems(List<OrderedItemDto> orderedItems)
        {
            if (orderedItems == null || orderedItems.Count() == 0)
            {
                return false;
            }
            foreach (OrderedItemDto item in orderedItems)
            {
                if (!ValidateOrderedItem(item))
                {
                    return false;
                }
            }
            return true;
        }

        private bool ValidateOrderedItem(OrderedItemDto item)
        {
            //price of 0.00 may be accaptable under certain circumstances (discount codes etc)
            return item.Quantity > 0 && item.Price >= 0;
        }

        //could also have edit order and cancel order however not in brief, implement later if time
    }
}