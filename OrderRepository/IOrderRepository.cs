using Order.Repository.Data;
using Order.Repository.Models;
using OrderData;
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

        public Task<IList<BasketItemRepoModel>> GetBasket(int customerId);

        public Task<CustomerRepoModel> GetCustomer(int customerId);

        public Task<IList<OrderRepoModel>> GetCustomerOrders(int customerId);

        public Task<int> CreateOrder(FinalisedOrderRepoModel order);

        public Task<bool> ClearBasket(int customerId);

        public Task<bool> CustomerExists(int customerId);

        public Task<bool> ProductsExist(List<ProductRepoModel> products);

        public Task<bool> ProductExists(ProductRepoModel product);
       
        public Task<bool> ProductExists(int productId);

        public Task<bool> ProductsInStock(List<ProductRepoModel> products);

        public Task<OrderRepoModel> GetCustomerOrder(int? orderId);

        public Task<bool> IsCustomerActive(int customerId);

        public Task<bool> IsItemInBasket(int customerId, int productId);

        public Task<int> NewCustomer(CustomerRepoModel customer);

        public Task<bool> EditCustomer(CustomerRepoModel customer);

        public Task<bool> AnonymiseCustomer(CustomerRepoModel customer);

        public Task<bool> CreateProduct(ProductRepoModel product);

        public Task<bool> EditProduct(ProductRepoModel product);

        public Task<bool> DeleteProduct(int productId);

        public Task<bool> DeleteOrder(int orderId);
    }
}
