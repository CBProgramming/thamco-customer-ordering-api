using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CustomerOrderingService.Models;
using Invoicing.Facade;
using Invoicing.Facade.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Order.Repository;
using Order.Repository.Data;
using Order.Repository.Models;
using Review.Facade;
using Review.Facade.Models;
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
        private readonly IInvoiceFacade _invoiceFacade;
        private readonly IReviewFacade _reviewFacade;
        private string authId, role;


        public OrderController(ILogger<OrderController> logger, IOrderRepository orderRepository, IMapper mapper,
            IStaffProductFacade staffProductFacade, IInvoiceFacade invoiceFacade, IReviewFacade reviewFacade)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _mapper = mapper;
            _staffProductFacade = staffProductFacade;
            _invoiceFacade = invoiceFacade;
            _reviewFacade = reviewFacade;
        }

        private void GetTokenDetails()
        {
            authId = User
                .Claims
                .FirstOrDefault(c => c.Type == "sub")?.Value;
            role = User
                .Claims
                .FirstOrDefault(c => c.Type == "role")?.Value;
        }

        [HttpGet("{customerId}")]
        [Authorize(Policy = "CustomerOrStaffWebApp")]
        public async Task<IActionResult> Get([FromRoute] int customerId, [FromQuery] int? orderId)
        {
            GetTokenDetails();
            var customer = _mapper.Map <CustomerDto>(await _orderRepository.GetCustomer(customerId));
            if (customer == null || !customer.Active)
            {
                return NotFound();
            }
            if (role == "Customer" && customer.CustomerAuthId != authId)
            {
                return Forbid();
            }
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
                if (order != null)
                {
                    //order.OrderedItems = _mapper.Map<List<OrderedItemDto>>(await _orderRepository.GetOrderItems(orderId));
                    order.OrderedItems.ForEach(p => p.OrderId = orderId ?? default);
                    return Ok(order);
                }
            }
            return NotFound();
        }

        [HttpPost]
        [Authorize(Policy = "CustomerOnly")]
        public async Task<IActionResult> Create(FinalisedOrderDto order)
        {
            GetTokenDetails();
            if (!ValidateOrder(order))
            {
                return UnprocessableEntity();
            }
            var customer = _mapper.Map<CustomerDto>(await _orderRepository.GetCustomer(order.CustomerId));
            if (customer == null || !customer.Active)
            {
                return NotFound();
            }
            if ((role == "Customer" && customer.CustomerAuthId != authId) 
                || !ValidContactDetails(customer) 
                || !customer.CanPurchase)
            {
                return Forbid();
            }
            if (!await _orderRepository.ProductsExist(_mapper.Map<List<ProductRepoModel>>(order.OrderedItems)))
            {
                return NotFound();
            }
            if (!await _orderRepository.ProductsInStock(_mapper.Map<List<ProductRepoModel>>(order.OrderedItems)))
            {
                return Conflict();
            }
            //reduce stock before creating order (it's worse customer service to allow a customer to order something out of stock
            //than for the company to innacurately display stock levels as lower than they are if an order fails
            var stockReductionList = GenerateStockReductions(order);
            if (!await _staffProductFacade.UpdateStock(stockReductionList))
            {
                //return NotFound();
            }
            order.OrderDate = ValidateDate(order.OrderDate);
            order.OrderId = await _orderRepository.CreateOrder(_mapper.Map<FinalisedOrderRepoModel>(order));
            if (order.OrderId == 0)
            {
                return NotFound();
            }
            if (!await _invoiceFacade.NewOrder(_mapper.Map<OrderInvoiceDto>(order)))
            {
                await _orderRepository.DeleteOrder(order.OrderId);
                return NotFound();
            }
            PurchaseDto purchases = _mapper.Map<PurchaseDto>(order);
            purchases.CustomerAuthId = authId;
            if (!await _reviewFacade.NewPurchases(purchases))
            {
                //record to local db to attempt resend later
                //insufficient time to implement however system continues to function
                //customer service issue as customer cannot leave review
            }
            await _orderRepository.ClearBasket(order.CustomerId);
            //return ok regardless of if the basket successfully clears because the order is complete
            //better customer service than clearing basket only to have order fail and customer needs 
            //to re-add everything to basket
            return Ok();
            
        }
                                


        private bool ValidContactDetails(CustomerDto customer)
        {
            return !String.IsNullOrEmpty(customer.AddressOne)
                && !String.IsNullOrEmpty(customer.AreaCode)
                && !String.IsNullOrEmpty(customer.Country)
                && !String.IsNullOrEmpty(customer.TelephoneNumber);
        }

        private bool ValidateOrder(FinalisedOrderDto order)
        {
            if (order == null || order.Total < 0)
            {
                return false;
            }
            if (order.OrderedItems == null || order.OrderedItems.Count() == 0)
            {
                return false;
            }
            foreach (OrderedItemDto item in order.OrderedItems)
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

        private List<StockReductionDto> GenerateStockReductions (FinalisedOrderDto order)
        {
            var stockReductionList = new List<StockReductionDto>();
            foreach (OrderedItemDto item in order.OrderedItems)
            {
                stockReductionList.Add(_mapper.Map<StockReductionDto>(item));
            }
            return stockReductionList;
        }

        private DateTime ValidateDate (DateTime orderDate)
        {
            //if date is over 7 days old, or a future date, set date to now (7 days chosen arbitrarily as it would 
            //likely be a business decision above my position)
            if (DateTime.Now.Ticks - orderDate.Ticks > (TimeSpan.TicksPerDay * 7) || orderDate > DateTime.Now)
            {
                return DateTime.Now;
            }
            return orderDate;
        }

        //could also have edit order and cancel order however not in brief, implement later if time
    }
}