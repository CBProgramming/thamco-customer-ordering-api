using AutoMapper;
using CustomerOrderingService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Order.Repository;
using Order.Repository.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerOrderingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "CustomerProductAPI")]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public ProductController(ILogger<ProductController> logger, IOrderRepository orderRepository, IMapper mapper)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        // PUT api/<controller>/5
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductDto product)
        {
            return await CreateOrEditProduct(product);
        }

        // PUT api/<controller>/5
        [HttpPut("{customerId}")]
        public async Task<IActionResult> Put([FromBody] ProductDto product)
        {
            return await CreateOrEditProduct(product);
        }

        private async Task<IActionResult> CreateOrEditProduct(ProductDto product)
        {
            if (product != null)
            {
                if (await _orderRepository.ProductExists(product.ProductId))
                {
                    if (await _orderRepository.EditProduct(_mapper.Map<ProductRepoModel>(product)))
                    {
                        return Ok();
                    }
                }
                else
                {
                    if (await _orderRepository.CreateProduct(_mapper.Map<ProductRepoModel>(product)))
                    {
                        return Ok();
                    }
                }
                return NotFound();
            }
            return UnprocessableEntity();
        }


        // DELETE api/<controller>/5
        [HttpDelete("{customerId}")]
        public async Task<IActionResult> Delete([FromRoute] int productId)
        {
            if (await _orderRepository.ProductExists(productId))
            {
                if (await _orderRepository.DeleteProduct(productId))
                {
                    return Ok();
                }
            }
            return NotFound();
        }
    }
}
