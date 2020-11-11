using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CustomerOrderingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        [HttpPost]
        public Task<IActionResult> Create(int customerId)
        {
            //create order, create FK relationships between order and product
            //then delete all products from the customer's basket
            throw new NotImplementedException();
        }

        //could also have edit order and cancel order however not in brief, implement later if time
    }
}