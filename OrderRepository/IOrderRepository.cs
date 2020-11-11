using OrderRepository.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository
{
    public interface IOrderRepository
    {
        public Task<bool> AddProduct(int customerId, ProductEFModel product)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditProduct(int customerId, int productId, int quantity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteProduct(int customerId, int productId)
        {
            throw new NotImplementedException();
        }

        public Task<IList<ProductEFModel>> GetBasket(int customerId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> FinaliseOrder(int customerId)
        {
            throw new NotImplementedException();
        }
    }
}
