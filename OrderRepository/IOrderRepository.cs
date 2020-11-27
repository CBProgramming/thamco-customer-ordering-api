using Order.Repository.Data;
using Order.Repository.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Order.Repository
{
    public interface IOrderRepository
    {
        public Task<bool> AddBasketItem(BasketItemEFModel newItem);

        public Task<bool> EditBasketItem(BasketItemEFModel editedItem);

        public Task<bool> DeleteBasketItem(int customerId, int productId);

        public Task<IList<BasketProductsEFModel>> GetBasket(int customerId);

        public Task<bool> FinaliseOrder(int customerId);

        public Task<CustomerEFModel> GetCustomer(int customerId);

        public Task<IList<OrderEFModel>> GetCustomerOrders(int customerId);

        public Task<IList<OrderedItemEFModel>> GetOrderItems(int? orderId);

        public Task<bool> CreateOrder(FinalisedOrderEFModel order);

        public Task<bool> ClearBasket(int customerId);

        public Task<bool> CustomerExists(int customerId);

        public Task<bool> ProductsExist(List<ProductEFModel> products);

        public Task<bool> ProductExists(ProductEFModel product);

        public Task<bool> ProductExists(int productId);

        public Task<bool> OrderExists(int? orderId);

        public Task<bool> ProductsInStock(List<ProductEFModel> products);

        public Task<bool> ProductInStock(ProductEFModel product);

        public Task<OrderEFModel> GetCustomerOrder(int? orderId);

        public Task<bool> IsCustomerActive(int customerId);

        public Task<bool> CanCustomerPurchase(int customerId);
    }
}
