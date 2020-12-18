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
        public async Task<IActionResult> Post([FromBody] List<ProductDto> products)
        {
            return await CreateOrEditProduct(products);
        }

        // PUT api/<controller>/5
        [HttpPut("{customerId}")]
        public async Task<IActionResult> Put([FromBody] List<ProductDto> products)
        {
            return await CreateOrEditProduct(products);
        }

        private async Task<IActionResult> CreateOrEditProduct(List<ProductDto> products)
        {
            if (products !=null && products.Count != 0)
            {
                foreach (ProductDto product in products)
                {
                    if (product == null)
                    {
                        return UnprocessableEntity();
                    }
                }
                var productsToRetry = new List<ProductDto>();
                foreach (ProductDto product in products)
                {
                    if (await _orderRepository.ProductExists(product.ProductId))
                    {
                        if (! await _orderRepository.EditProduct(_mapper.Map<ProductRepoModel>(product)))
                        {
                            productsToRetry.Add(product);
                        }
                    }
                    else
                    {
                        if (!await _orderRepository.CreateProduct(_mapper.Map<ProductRepoModel>(product)))
                        {
                            productsToRetry.Add(product);
                        }
                    }
                }
                if (productsToRetry.Count == 0)
                {
                    return Ok();
                }
                else if (productsToRetry.Count == products.Count)
                {
                    return NotFound();
                }
                else
                {
                    //as this only occurs if a partial number of products doesn't post
                    //there is no risk of an infinite loop as the retry list is always smaller
                    //this could still be very problematic if the connection is poor and the list is long
                    return await CreateOrEditProduct(productsToRetry);
                }
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
