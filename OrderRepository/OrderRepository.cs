using Order.Repository.Data;
using OrderData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Order.Repository.Models;
using AutoMapper;

namespace Order.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDb _context;
        private readonly IMapper _mapper;

        public OrderRepository(OrderDb context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> AddBasketItem(BasketItemModel newItem)
        {
            if (await ProductDetailsCheck(_mapper.Map<ProductEFModel>(newItem)))
            {
                return await AddToBasket(_mapper.Map<BasketItemModel>(newItem));
            }
            else
            {
                return false;
            }
        }

        private async Task<bool> AddToBasket(BasketItemModel basketItem)
        {
            if (await IsItemInBasket(basketItem.CustomerId, basketItem.ProductId))
            {
                return await EditBasketItem(basketItem);
            }
            else
            {
                try
                {
                    _context.Add(_mapper.Map<BasketItem>(basketItem));
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }

        public async Task<bool> EditBasketItem(BasketItemModel editedItem)
        {
            if (await ProductDetailsCheck(_mapper.Map<ProductEFModel>(editedItem)))
            {
                return await EditItemInBasket(_mapper.Map<BasketItemModel>(editedItem));
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> DeleteBasketItem(int customerId, int productId)
        {
            try
            {
                var item = _context.BasketItems.SingleOrDefault(
                b => b.CustomerId == customerId && b.ProductId == productId);
                if (item != null)
                {
                    _context.BasketItems.Remove(item);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<IList<BasketProductsModel>> GetBasket(int customerId)
        {
            var basketItems = _context.BasketItems
                .Where(b => b.CustomerId == customerId)
                .Join(_context.Products,b => b.ProductId, p => p.ProductId,
                (basketItem, product) => new BasketProductsModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = basketItem.Quantity
                });
            return basketItems == null ? null : basketItems.ToList();
        }

        public Task<bool> FinaliseOrder(int customerId)
        {
            throw new NotImplementedException();
        }

        private async Task<bool> ProductDetailsCheck(ProductEFModel product)
        {
            if (ProductExists(product.ProductId))
            {
                return await CreateProduct(_mapper.Map<ProductEFModel>(product));
            }
            else
            {
                return await EditProduct(_mapper.Map<ProductEFModel>(product));
            }
        }

        public bool ProductExists(int id)
        {
            return _context.Products.Any(p => p.ProductId == id);
        }

        private async Task<bool> CreateProduct(ProductEFModel product)
        {
            try
            {
                _context.Add(_mapper.Map<Product>(product));
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        private async Task<bool> EditProduct(ProductEFModel product)
        {
            try
            {
                _context.Update(_mapper.Map<Product>(product));
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        private async Task<bool> EditItemInBasket(BasketItemModel basketItemModel)
        {
            try
            {
                _context.Update(_mapper.Map<BasketItem>(basketItemModel));
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        private async Task<bool> IsItemInBasket(int customerId, int productId)
        {
            return _context.BasketItems.SingleOrDefault(
                b => b.CustomerId == customerId && b.ProductId == productId) != null;
        }

        public async Task<CustomerEFModel> GetCustomer(int customerId)
        {
            return _mapper.Map<CustomerEFModel>(_context.Customers.SingleOrDefault(c => c.CustomerId == customerId));
        }

        public async Task<IList<OrderEFModel>> GetCustomerOrders(int customerId)
        {
            return _mapper.Map<List<OrderEFModel>>(_context.Orders.Where(o => o.CustomerId == customerId));
        }

        public async Task<IList<OrderedItemEFModel>> GetOrderItems(int orderId)
        {
            return _mapper.Map<List<OrderedItemEFModel>>(_context.OrderedItems.Where(o => o.OrderId == orderId));
        }

        public async Task<bool> CreateOrder(FinalisedOrderEFModel finalisedOrder)
        {
            try
            {
                var order = _mapper.Map<OrderData.Order>(finalisedOrder);
                _context.Add(order);
                foreach (OrderedItemEFModel orderedItem in finalisedOrder.OrderedItems)
                {
                    var item = new OrderedItem
                    {
                        Order = order,
                        ProductId = orderedItem.ProductId,
                        Quantity = orderedItem.Quantity,
                        Price = orderedItem.Price,
                        Name = orderedItem.Name
                    };
                    _context.Add(item);
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<bool> ClearBasket(int customerId)
        {
            try
            {
                foreach (BasketItem item in _context.BasketItems.Where(b => b.CustomerId == customerId))
                {
                    _context.BasketItems.Remove(item);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }
    }
}
