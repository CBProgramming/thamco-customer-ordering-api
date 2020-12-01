using Order.Repository.Data;
using Order.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Repository
{
    public class FakeOrderRepository : IOrderRepository
    {
        public bool CompletesOrders = true;
        public bool AcceptsBasketItems = true;
        public bool AcceptsDeletions = true;
        public CustomerEFModel Customer { get; set; }

        public List<OrderEFModel> Orders { get; set; }

        public List<OrderedItemEFModel> OrderedItems { get; set; }

        public List<ProductEFModel> Products { get; set; }

        public List<BasketProductsEFModel> CurrentBasket { get; set; }

        public async Task<bool> ClearBasket(int customerId)
        {
            return await CustomerExists(customerId);
        }

        public async Task<bool> CreateOrder(FinalisedOrderEFModel order)
        {
            if (await CustomerExists(order.CustomerId) 
                && order.OrderedItems.Count > 0
                && CompletesOrders)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> CustomerExists(int customerId)
        {
            return customerId == Customer.CustomerId;
        }

        public async Task<CustomerEFModel> GetCustomer(int customerId)
        {
            if (await CustomerExists(customerId))
            {
                return Customer;
            }
            return null;
        }

        public async Task<IList<OrderEFModel>> GetCustomerOrders(int customerId)
        {
            return Orders;
        }

        public async Task<IList<OrderedItemEFModel>> GetOrderItems(int orderId)
        {
            return OrderedItems;
        }

        public async Task<bool> ProductsExist(List<ProductEFModel> products)
        {
            foreach (ProductEFModel product in products)
            {
                if (! await ProductExists(product))
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> ProductExists(ProductEFModel product)
        {
            foreach (ProductEFModel productInStock in Products)
            {
                if (productInStock.ProductId == product.ProductId)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> ProductsInStock(List<ProductEFModel> products)
        {
            foreach (ProductEFModel product in products)
            {
                if (! await ProductInStock(product))
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> ProductInStock(ProductEFModel product)
        {
            foreach (ProductEFModel productInStock in Products)
            {
                if (productInStock.ProductId == product.ProductId)
                {
                    return productInStock.Quantity >= product.Quantity;
                }
            }
            return false;
        }

        public async Task<bool> AddBasketItem(BasketItemEFModel newItem)
        {
            return AcceptsBasketItems;
        }

        public async Task<bool> EditBasketItem(BasketItemEFModel editedItem)
        {
            return AcceptsBasketItems;
        }

        public async Task<bool> DeleteBasketItem(int customerId, int productId)
        {
            return AcceptsBasketItems;
        }

        public async Task<IList<BasketProductsEFModel>> GetBasket(int customerId)
        {
            return CurrentBasket;
        }

        public Task<bool> FinaliseOrder(int customerId)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<OrderedItemEFModel>> GetOrderItems(int? orderId)
        {
            return OrderedItems;
        }

        public async Task<OrderEFModel> GetCustomerOrder(int? orderId)
        {
            if (orderId != null)
            {
                return Orders[orderId ?? default];
            }
            return null;
        }

        public async Task<bool> OrderExists(int orderId)
        {
            return Orders.Any(o => o.OrderId == orderId);
        }

        public async Task<bool> IsCustomerActive(int customerId)
        {
            return Customer.Active;
        }

        public async Task<bool> CanCustomerPurchase(int customerId)
        {
            return Customer.CanPurchase;
        }

        public async Task<bool> OrderExists(int? orderId)
        {
            return Orders.Any(o => o.OrderId == orderId);
        }

        public async Task<bool> ProductExists(int productId)
        {
            return Products.Any(p => p.ProductId == productId);
        }

        public async Task<bool> IsItemInBasket(int customerId, int productId)
        {
            return CurrentBasket.Any(b => b.ProductId == productId);
        }

        public Task<bool> NewCustomer(CustomerEFModel customer)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditCustomer(CustomerEFModel customer)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AnonymiseCustomer(CustomerEFModel customer)
        {
            throw new NotImplementedException();
        }
    }
}
