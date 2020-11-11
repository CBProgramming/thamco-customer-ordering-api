using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerOrderingService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly ILogger<BasketController> _logger;

        public BasketController(ILogger<BasketController> logger)
        {
            _logger = logger;
        }

        //Get basket
        [HttpPost]
        public Task<IActionResult> Get(int customerId)
        {
            throw new NotImplementedException();
        }

        //Add item to basket
        [HttpPost]
        public Task<IActionResult> Create(int customerId, int productId)
        {
            throw new NotImplementedException();
        }

        //Edit product in basket
        [HttpPut]
        public Task<IActionResult> Edit(int customerId, int productId, int quantity)
        {
            throw new NotImplementedException();
        }

        //Remove item from basket
        [HttpDelete]
        public Task<IActionResult> Delete(int customerId, int productId)
        {
            throw new NotImplementedException();
        }
    }
}
