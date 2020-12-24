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

        public async Task<CustomerRepoModel> GetCustomer(int customerId)
        {
            return _mapper.Map<CustomerRepoModel>(_context
                .Customers
                .Where(c => c.Active == true)
                .FirstOrDefault(c => c.CustomerId == customerId));
        }

        public async Task<int> NewCustomer(CustomerRepoModel newCustomer)
        {
            if (newCustomer != null)
            {
                try
                {
                    var customer = _mapper.Map<Customer>(newCustomer);
                    _context.Add(customer);
                    await _context.SaveChangesAsync();
                    return customer.CustomerId;
                }
                catch (DbUpdateConcurrencyException)
                {

                }
            }
            return 0;
        }

        public async Task<bool> EditCustomer(CustomerRepoModel editedCustomer)
        {
            if (editedCustomer != null)
            {
                var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == editedCustomer.CustomerId);
                if (customer != null)
                {
                    try
                    {
                        customer.GivenName = editedCustomer.GivenName;
                        customer.FamilyName = editedCustomer.FamilyName;
                        customer.AddressOne = editedCustomer.AddressOne;
                        customer.AddressTwo = editedCustomer.AddressTwo;
                        customer.Town = editedCustomer.Town;
                        customer.State = editedCustomer.State;
                        customer.AreaCode = editedCustomer.AreaCode;
                        customer.Country = editedCustomer.Country;
                        customer.EmailAddress = editedCustomer.EmailAddress;
                        customer.TelephoneNumber = editedCustomer.TelephoneNumber;
                        customer.CanPurchase = editedCustomer.CanPurchase;
                        customer.Active = editedCustomer.Active;
                        await _context.SaveChangesAsync();
                        return true;
                    }
                    catch (DbUpdateConcurrencyException)
                    {

                    }
                }
            }
            return false;
        }

        public async Task<bool> AnonymiseCustomer(CustomerRepoModel anonCustomer)
        {
            return await EditCustomer(anonCustomer);
        }

        public async Task<IList<BasketItemRepoModel>> GetBasket(int customerId)
        {
            var result = _context.BasketItems
                .Where(b => b.CustomerId == customerId)
                .Join(_context.Products,
                b => b.ProductId,
                p => p.ProductId,
                (basketItem, product) => new BasketItemRepoModel
                {
                    CustomerId = customerId,
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = basketItem.Quantity
                });
            var listresult = result.ToList();
            return listresult;
        }

        public async Task<bool> AddBasketItem(BasketItemRepoModel basketItem)
        {
            if (basketItem != null 
                && await ProductExists(basketItem.ProductId)
                && await CustomerExists(basketItem.CustomerId))
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
                    catch (DbUpdateConcurrencyException)
                    {
                        
                    }
                }
            }
            return false;
        }

        public async Task<bool> EditBasketItem(BasketItemRepoModel basketItem)
        {
            if (basketItem != null)
            {
                if (!await IsItemInBasket(basketItem.CustomerId, basketItem.ProductId))
                {
                    return await AddBasketItem(basketItem);
                }
                else
                {
                    var item = GetBasketItem(basketItem.CustomerId, basketItem.ProductId);
                    if (item != null)
                    {
                        try
                        {
                            item.Quantity = basketItem.Quantity;
                            await _context.SaveChangesAsync();
                            return true;
                        }
                        catch (DbUpdateConcurrencyException)
                        {

                        }
                    }
                }
            }
            return false;
        }

        public async Task<bool> IsItemInBasket(int customerId, int productId)
        {
            var basketItem = GetBasketItem(customerId, productId);
            if (basketItem != null)
            {
                return true;
            }
            return false;
        }

        private BasketItem GetBasketItem(int customerId, int productId)
        {
            return _context.BasketItems.Where(b => b.CustomerId == customerId && b.ProductId == productId).FirstOrDefault();
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
                    return true;
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                
            }
            return false;
        }

        public async Task<bool> ProductExists(int id)
        {
            return _context.Products.Any(p => p.ProductId == id);
        }

        public async Task<bool> CreateProduct(ProductRepoModel product)
        {
            if (product != null)
            {
                try
                {
                    var customer = _mapper.Map<Product>(product);
                    _context.Add(customer);
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateConcurrencyException)
                {

                }
            }
            return false;
        }

        public async Task<bool> EditProduct(ProductRepoModel productModel)
        {
            if (productModel != null)
            {
                var product = _context.Products.FirstOrDefault(p => p.ProductId == productModel.ProductId);
                if (product != null)
                {
                    try
                    {
                        product.Name = productModel.Name;
                        product.Price = productModel.Price;
                        product.Quantity += productModel.Quantity;
                        await _context.SaveChangesAsync();
                        return true;
                    }
                    catch (DbUpdateConcurrencyException)
                    {

                    }
                }
            }
            return false;
        }

        public async Task<bool> DeleteProduct(int productId)
        {
            try
            {
                var item = _context.Products.SingleOrDefault(p => p.ProductId == productId);
                if (item != null)
                {
                    _context.Products.Remove(item);
                    await _context.SaveChangesAsync();
                    return true;
                }
            }
            catch (DbUpdateConcurrencyException)
            {

            }
            return false;
        }

        public async Task<bool> FinaliseOrder(int customerId)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<OrderRepoModel>> GetCustomerOrders(int customerId)
        {
            return _mapper.Map<List<OrderRepoModel>>(_context.Orders.Where(o => o.CustomerId == customerId));
        }

        public async Task<OrderRepoModel> GetCustomerOrder(int? orderId)
        {

            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            var orderedItems = _context.OrderedItems.Where(o => o.OrderId == orderId).ToList();
            if (order == null || orderedItems == null || orderedItems.Count<1)
            {
                return null;
            }
            order.OrderedItems = orderedItems;
            var mappedOrder = _mapper.Map<OrderRepoModel>(order);
            return mappedOrder;

        }

        public async Task<IList<OrderedItemRepoModel>> GetOrderItems(int? orderId)
        {
            return _mapper.Map<List<OrderedItemRepoModel>>(_context.OrderedItems.Where(o => o.OrderId == orderId));
        }

        public async Task<int> CreateOrder(FinalisedOrderRepoModel finalisedOrder)
        {
            if (finalisedOrder == null || finalisedOrder.OrderedItems == null || finalisedOrder.OrderedItems.Count <1)
            {
                return 0;
            }
            foreach (OrderedItemRepoModel orderedItem in finalisedOrder.OrderedItems)
            {
                if (orderedItem == null)
                {
                    return 0;
                }
            }
            try
            {
                finalisedOrder.OrderId = 0;
                var order = _mapper.Map<OrderData.Order>(finalisedOrder);
                _context.Add(order);
                foreach (OrderData.OrderedItem orderedItem in order.OrderedItems)
                {
                    var product = _context.Products.FirstOrDefault(product => product.ProductId == orderedItem.ProductId);
                    if (product != null)
                    {
                        int newStock = product.Quantity - orderedItem.Quantity;
                        product.Quantity = newStock;
                    }
                }
                await _context.SaveChangesAsync();
                return order.OrderId;
            }
            catch (DbUpdateConcurrencyException)
            {
                return 0;
            }
        }

        public async Task<bool> ClearBasket(int customerId)
        {
            try
            {
                foreach (BasketItem item in _context.BasketItems.Where(b => b.CustomerId == customerId))
                {
                    _context.BasketItems.Remove(item);
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<bool> CustomerExists(int customerId)
        {
            return _context.Customers.Any(c => c.CustomerId == customerId);
        }

        public async Task<bool> IsCustomerActive(int customerId)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == customerId);
            if (customer != null)
            {
                return customer.Active;
            }
            return false;
        }

        public async Task<bool> ProductsExist(List<ProductRepoModel> products)
        {
            if (products == null || products.Count < 1)
            {
                return false;
            }
            foreach (ProductRepoModel product in products)
            {
                if (product == null || ! await ProductExists(product))
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> ProductExists(ProductRepoModel product)
        {
            return _context.Products.Any(p => p.ProductId == product.ProductId);
        }

        public async Task<bool> ProductsInStock(List<ProductRepoModel> products)
        {
            if (products == null || products.Count < 1)
            {
                return false;
            }
            foreach (ProductRepoModel product in products)
            {
                if (product == null || ! await ProductInStock(product))
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> ProductInStock(ProductRepoModel product)
        {
            return product.Quantity <= _context.Products.FirstOrDefault(p => p.ProductId == product.ProductId).Quantity;
        }

        public async Task<bool> OrderExists(int? orderId)
        {
            return _context.Orders.Any(o => o.OrderId == orderId);
        }

        public Task<bool> ContactDetailsSufficient(int customerId)
        {
            throw new NotImplementedException();
        }

    }
}
