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
        public Task<bool> AddBasketItem(BasketItemRepoModel newItem);

        public Task<bool> EditBasketItem(BasketItemRepoModel editedItem);

        public Task<bool> DeleteBasketItem(int customerId, int productId);

        public Task<IList<BasketProductsRepoModel>> GetBasket(int customerId);

        public Task<bool> FinaliseOrder(int customerId);

        public Task<CustomerRepoModel> GetCustomer(int customerId);

        public Task<IList<OrderRepoModel>> GetCustomerOrders(int customerId);

        public Task<IList<OrderedItemRepoModel>> GetOrderItems(int? orderId);

        public Task<bool> CreateOrder(FinalisedOrderRepoModel order);

        public Task<bool> ClearBasket(int customerId);

        public Task<bool> CustomerExists(int customerId);

        public Task<bool> ProductsExist(List<ProductRepoModel> products);

        public Task<bool> ProductExists(ProductRepoModel product);

        public Task<bool> ProductExists(int productId);

        public Task<bool> OrderExists(int? orderId);

        public Task<bool> ProductsInStock(List<ProductRepoModel> products);

        public Task<bool> ProductInStock(ProductRepoModel product);

        public Task<OrderRepoModel> GetCustomerOrder(int? orderId);

        public Task<bool> IsCustomerActive(int customerId);

        public Task<bool> CanCustomerPurchase(int customerId);

        public Task<bool> IsItemInBasket(int customerId, int productId);

        public Task<bool> NewCustomer(CustomerRepoModel customer);

        public Task<bool> EditCustomer(CustomerRepoModel customer);

        public Task<bool> AnonymiseCustomer(CustomerRepoModel customer);
    }
}
