using Order.Repository.Data;
using Order.Repository.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Order.Repository
{
    public class FakeOrderRepository : IOrderRepository
    {
        public CustomerEFModel Customer { get; set; }

        public List<OrderEFModel> Orders { get; set; }

        public List<OrderedItemEFModel> OrderedItems { get; set; }

        public List<ProductEFModel> Products { get; set; }

        public async Task<bool> ClearBasket(int customerId)
        {
            return CustomerExists(customerId);
        }

        public async Task<bool> CreateOrder(FinalisedOrderEFModel order)
        {
            if (CustomerExists(order.CustomerId) 
                && order.OrderedItems.Count > 0)
            {
                return true;
            }
            return false;
        }

        public bool CustomerExists(int customerId)
        {
            return customerId == Customer.CustomerId;
        }

        public async Task<CustomerEFModel> GetCustomer(int customerId)
        {
            if (CustomerExists(customerId))
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

        public bool ProductsExist(List<ProductEFModel> products)
        {
            foreach (ProductEFModel product in products)
            {
                if (!ProductExists(product))
                {
                    return false;
                }
            }
            return true;
        }

        public bool ProductExists(ProductEFModel product)
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

        public bool ProductsInStock(List<ProductEFModel> products)
        {
            foreach (ProductEFModel product in products)
            {
                if (!ProductInStock(product))
                {
                    return false;
                }
            }
            return true;
        }

        public bool ProductInStock(ProductEFModel product)
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

        public Task<bool> AddBasketItem(BasketItemModel newItem)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditBasketItem(BasketItemModel editedItem)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteBasketItem(int customerId, int productId)
        {
            throw new NotImplementedException();
        }

        public Task<IList<BasketProductsModel>> GetBasket(int customerId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> FinaliseOrder(int customerId)
        {
            throw new NotImplementedException();
        }

        public Task<IList<OrderedItemEFModel>> GetOrderItems(int? orderId)
        {
            throw new NotImplementedException();
        }

        public Task<OrderEFModel> GetCustomerOrder(int? orderId)
        {
            throw new NotImplementedException();
        }
    }
}
