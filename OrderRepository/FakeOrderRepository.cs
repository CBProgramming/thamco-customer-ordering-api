using Order.Repository.Data;
using Order.Repository.Models;
using OrderData;
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
        public bool AutoFails = false;
        public bool autoSucceeds = false;
        public int FailureAmount = 0;

        public CustomerRepoModel Customer { get; set; }

        public ProductRepoModel Product { get; set; }

        public List<OrderRepoModel> Orders { get; set; }

        public List<OrderedItemRepoModel> OrderedItems { get; set; }

        public List<ProductRepoModel> Products { get; set; }

        public List<BasketItemRepoModel> CurrentBasket { get; set; }

        public FinalisedOrderRepoModel FinalisedOrder { get; set; }

        public async Task<bool> ClearBasket(int customerId)
        {
            return await CustomerExists(customerId);
        }

        public async Task<int> CreateOrder(FinalisedOrderRepoModel order)
        {
            if (await CustomerExists(order.CustomerId) 
                && order.OrderedItems.Count > 0
                && CompletesOrders)
            {
                FinalisedOrder = order;
                return order.OrderId==0? 1 : order.OrderId;
            }
            return 0;
        }

        public async Task<bool> CustomerExists(int customerId)
        {
            if (!AutoFails)
            {
                return (Customer != null && customerId == Customer.CustomerId);
            }
            return AutoFails;
        }

        public async Task<CustomerRepoModel> GetCustomer(int customerId)
        {
            if (!AutoFails && await CustomerExists(customerId))
            {
                return Customer;
            }
            return null;
        }

        public async Task<IList<OrderRepoModel>> GetCustomerOrders(int customerId)
        {
            if (!AutoFails)
            {
                return Orders;
            }
            return null;
        }

        public async Task<IList<OrderedItemRepoModel>> GetOrderItems(int orderId)
        {
            if (!AutoFails)
            {
                return OrderedItems;
            }
            return null;
        }

        public async Task<bool> ProductsExist(List<ProductRepoModel> products)
        {
            if (!AutoFails)
            {
                foreach (ProductRepoModel product in products)
                {
                    if (!await ProductExists(product))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public async Task<bool> ProductExists(ProductRepoModel product)
        {
            if (!AutoFails)
            {
                foreach (ProductRepoModel productInStock in Products)
                {
                    if (productInStock.ProductId == product.ProductId)
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }

        public async Task<bool> ProductsInStock(List<ProductRepoModel> products)
        {
            if (!AutoFails)
            {
                foreach (ProductRepoModel product in products)
                {
                    if (!await ProductInStock(product))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public async Task<bool> ProductInStock(ProductRepoModel product)
        {
            if (!AutoFails)
            {
                foreach (ProductRepoModel productInStock in Products)
                {
                    if (productInStock.ProductId == product.ProductId)
                    {
                        return productInStock.Quantity >= product.Quantity;
                    }
                }
                return false;
            }
            return false;
        }

        public async Task<bool> AddBasketItem(BasketItemRepoModel newItem)
        {
            if (!AutoFails)
            {
                return AcceptsBasketItems;
            }
            return false;
        }

        public async Task<bool> EditBasketItem(BasketItemRepoModel editedItem)
        {
            if (!AutoFails)
            {
                return AcceptsBasketItems;
            }
            return false;
        }

        public async Task<bool> DeleteBasketItem(int customerId, int productId)
        {
            if (!AutoFails)
            {
                return AcceptsBasketItems;
            }
            return false;
        }

        public async Task<IList<BasketItemRepoModel>> GetBasket(int customerId)
        {
            if (!AutoFails)
            {
                return CurrentBasket;
            }
            return null;
        }

        public async Task<bool> FinaliseOrder(int customerId)
        {
            if (!AutoFails)
            {
                return false;
            }
            return false;
        }

        public async Task<IList<OrderedItemRepoModel>> GetOrderItems(int? orderId)
        {
            if (!AutoFails)
            {
                return OrderedItems;
            }
            return null;
        }

        public async Task<OrderRepoModel> GetCustomerOrder(int? orderId)
        {
            if (!AutoFails && orderId != null && orderId <= Orders.Count -1 && orderId > -1)
            {
                var result = Orders[orderId ?? default];
                result.OrderedItems = OrderedItems;
                return result;
            }
            return null;
        }

        public async Task<bool> OrderExists(int orderId)
        {
            if (!AutoFails)
            {
                return Orders.Any(o => o.OrderId == orderId);
            }
            return false;
        }

        public async Task<bool> IsCustomerActive(int customerId)
        {
            if (!AutoFails)
            {
                return Customer.Active;
            }
            return false;
        }

        public async Task<bool> OrderExists(int? orderId)
        {
            if (!AutoFails)
            {
                return Orders.Any(o => o.OrderId == orderId);
            }
            return false;
        }

        public async Task<bool> ProductExists(int productId)
        {
            if (!AutoFails)
            {
                return Products.Any(p => p.ProductId == productId);
            }
            return false;
        }

        public async Task<bool> IsItemInBasket(int customerId, int productId)
        {
            return CurrentBasket.Any(b => b.ProductId == productId);
        }

        public async Task<int> NewCustomer(CustomerRepoModel customer)
        {
            if (!AutoFails)
            {
                Customer = customer;
                return customer.CustomerId;
            }
            return 0;
        }

        public async Task<bool> EditCustomer(CustomerRepoModel customer)
        {
            if (!AutoFails)
            {
                if (Customer != null && await CustomerExists(customer.CustomerId))
                {
                    Customer = customer;
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> AnonymiseCustomer(CustomerRepoModel customer)
        {
            if (!AutoFails)
            {
                if (Customer != null &&  await CustomerExists(customer.CustomerId))
                {
                    Customer = customer;
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> CreateProduct(ProductRepoModel product)
        {
            if (!AutoFails)
            {
                if (FailureAmount > 0)
                {
                    FailureAmount--;
                }
                else
                {
                    if (!await ProductExists(product))
                    {
                        Products.Add(product);
                        return true;
                    }
                    else
                    {
                        return await EditProduct(product);
                    }
                }
            }
            return false;
        }

        public async Task<bool> EditProduct(ProductRepoModel product)
        {
            if (!AutoFails)
            {
                if (FailureAmount > 0)
                {
                    FailureAmount--;
                }
                else
                {
                    if (!await ProductExists(product))
                    {
                        return await CreateProduct(product);
                    }
                    else
                    {
                        ProductRepoModel repoProduct = Products.Where(p => p.ProductId == product.ProductId).FirstOrDefault();
                        repoProduct.Name = product.Name;
                        repoProduct.Price = product.Price;
                        repoProduct.Quantity += product.Quantity;
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<bool> DeleteProduct(int productId)
        {
            if (!AutoFails && Product != null)
            {
                if (await ProductExists(productId))
                {
                    Products.Remove(Products.Where(p => p.ProductId == productId).FirstOrDefault());
                    return true;
                }
            }
            return false;
        }

        public Task<bool> ContactDetailsSufficient(int customerId)
        {
            throw new NotImplementedException();
        }
    }
}
