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

        public async Task<bool> AddBasketItem(BasketItemRepoModel newItem)
        {
            if (await ProductDetailsCheck(_mapper.Map<ProductRepoModel>(newItem)))
            {
                return await AddToBasket(_mapper.Map<BasketItemRepoModel>(newItem));
            }
            else
            {
                return false;
            }
        }

        private async Task<bool> AddToBasket(BasketItemRepoModel basketItem)
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

        public async Task<bool> EditBasketItem(BasketItemRepoModel editedItem)
        {
            if (await ProductDetailsCheck(_mapper.Map<ProductRepoModel>(editedItem)))
            {
                return await EditItemInBasket(_mapper.Map<BasketItemRepoModel>(editedItem));
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

        public async Task<IList<BasketProductsRepoModel>> GetBasket(int customerId)
        {
            var basketItems = _context.BasketItems
                .Where(b => b.CustomerId == customerId)
                .Join(_context.Products,b => b.ProductId, p => p.ProductId,
                (basketItem, product) => new BasketProductsRepoModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = basketItem.Quantity
                });
            return basketItems?.ToList();
        }

        public async Task<bool> FinaliseOrder(int customerId)
        {
            throw new NotImplementedException();
        }

        private async Task<bool> ProductDetailsCheck(ProductRepoModel product)
        {
            var productExists = await ProductExists(product.ProductId);
            if (productExists)
            {
                return await CreateProduct(_mapper.Map<ProductRepoModel>(product));
            }
            else
            {
                return await EditProduct(_mapper.Map<ProductRepoModel>(product));
            }
        }

        public async Task<bool> ProductExists(int id)
        {
            return _context.Products.Any(p => p.ProductId == id);
        }

        private async Task<bool> CreateProduct(ProductRepoModel product)
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

        private async Task<bool> EditProduct(ProductRepoModel product)
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

        private async Task<bool> EditItemInBasket(BasketItemRepoModel basketItemModel)
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

        public async Task<bool> IsItemInBasket(int customerId, int productId)
        {
            return _context.BasketItems.SingleOrDefault(
                b => b.CustomerId == customerId && b.ProductId == productId) != null;
        }

        public async Task<CustomerRepoModel> GetCustomer(int customerId)
        {
            return _mapper.Map<CustomerRepoModel>(_context
                .Customers
                .Where(c => c.Active == true)
                .FirstOrDefault(c => c.CustomerId == customerId));
        }

        public async Task<IList<OrderRepoModel>> GetCustomerOrders(int customerId)
        {
            return _mapper.Map<List<OrderRepoModel>>(_context.Orders.Where(o => o.CustomerId == customerId));
        }

        public async Task<OrderRepoModel> GetCustomerOrder(int? orderId)
        {
            return _mapper.Map<OrderRepoModel>(_context.Orders.Where(o => o.OrderId == orderId));
        }

        public async Task<IList<OrderedItemRepoModel>> GetOrderItems(int? orderId)
        {
            return _mapper.Map<List<OrderedItemRepoModel>>(_context.OrderedItems.Where(o => o.OrderId == orderId));
        }

        public async Task<bool> CreateOrder(FinalisedOrderRepoModel finalisedOrder)
        {
            try
            {
                var order = _mapper.Map<OrderData.Order>(finalisedOrder);
                _context.Add(order);
                foreach (OrderedItemRepoModel orderedItem in finalisedOrder.OrderedItems)
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

        public async Task<bool> CustomerExists(int customerId)
        {
            return _context.Customers.Any(c => c.CustomerId == customerId);
        }

        public async Task<bool> IsCustomerActive(int customerId)
        {
            return _context.Customers.FirstOrDefault(c => c.CustomerId == customerId).Active;
        }

        public async Task<bool> CanCustomerPurchase(int customerId)
        {
            return _context.Customers.FirstOrDefault(c => c.CustomerId == customerId).CanPurchase;
        }

        public async Task<bool> ProductsExist(List<ProductRepoModel> products)
        {
            foreach (ProductRepoModel product in products)
            {
                if (! await ProductExists(product))
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
            foreach (ProductRepoModel product in products)
            {
                if (! await ProductInStock(product))
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> ProductInStock(ProductRepoModel product)
        {
            return product.Quantity <= _mapper.Map<ProductRepoModel>(_context.Products.Any(p => p.ProductId == product.ProductId)).Quantity;
        }

        public async Task<bool> OrderExists(int? orderId)
        {
            return _context.Orders.Any(o => o.OrderId == orderId);
        }

        public async Task<bool> NewCustomer(CustomerRepoModel newCustomer)
        {
            if (newCustomer != null)
            {
                try
                {
                    var customer = _mapper.Map<Customer>(newCustomer);
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

        public async Task<bool> EditCustomer(CustomerRepoModel editedCustomer)
        {
            if (editedCustomer != null)
            {
                var customer = await _context.Customers.FindAsync(editedCustomer.CustomerId);
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
            return false;
        }

        public async Task<bool> AnonymiseCustomer(CustomerRepoModel anonCustomer)
        {
            return await EditCustomer(anonCustomer);
        }
    }
}
