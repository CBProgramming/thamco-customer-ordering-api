﻿using Order.Repository.Data;
using Order.Repository.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Order.Repository
{
    public interface IOrderRepository
    {
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
    }
}
