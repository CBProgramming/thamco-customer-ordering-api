﻿using Order.Repository.Data;
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
        public bool AutoFails = false;
        public bool autoSucceeds = false;

        public CustomerRepoModel Customer { get; set; }

        public List<OrderRepoModel> Orders { get; set; }

        public List<OrderedItemRepoModel> OrderedItems { get; set; }

        public List<ProductRepoModel> Products { get; set; }

        public List<BasketProductsRepoModel> CurrentBasket { get; set; }

        public async Task<bool> ClearBasket(int customerId)
        {
            return await CustomerExists(customerId);
        }

        public async Task<bool> CreateOrder(FinalisedOrderRepoModel order)
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

        public async Task<IList<BasketProductsRepoModel>> GetBasket(int customerId)
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
            if (!AutoFails && orderId != null)
            {
                return Orders[orderId ?? default];
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

        public async Task<bool> CanCustomerPurchase(int customerId)
        {
            if (!AutoFails)
            {
                return Customer.CanPurchase;
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

        public async Task<bool> NewCustomer(CustomerRepoModel customer)
        {
            if (!AutoFails)
            {
                Customer = customer;
                return true;
            }
            return false;
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
    }
}
