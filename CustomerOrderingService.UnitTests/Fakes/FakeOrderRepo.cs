using Order.Repository;
using Order.Repository.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CustomerOrderingService.UnitTests.Fakes
{
    public class FakeOrderRepo : IOrderRepository
    {
        public CustomerEFModel Customer { get; set; }

        public List<OrderEFModel> Orders { get; set; }

        public List<OrderedItemEFModel> OrderedItems { get; set; }

        public async Task<bool> ClearBasket(int customerId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CreateOrder(FinalisedOrderEFModel order)
        {
            throw new NotImplementedException();
        }

        public async Task<CustomerEFModel> GetCustomer(int customerId)
        {
            return Customer;
        }

        public async Task<IList<OrderEFModel>> GetCustomerOrders(int customerId)
        {
            return Orders;
        }

        public async Task<IList<OrderedItemEFModel>> GetOrderItems(int orderId)
        {
            return OrderedItems;
        }
    }
}
