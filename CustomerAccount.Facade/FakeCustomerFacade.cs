using CustomerAccount.Facade.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CustomerAccount.Facade
{
    public class FakeCustomerFacade : ICustomerAccountFacade
    {
        public bool Succeeds = true;
        public int CustomerId;
        public CustomerFacadeDto Customer;

        public async Task<bool> DeleteCustomer(int customerId)
        {
            CustomerId = customerId;
            return Succeeds;
        }

        public async Task<bool> EditCustomer(CustomerFacadeDto editedCustomer)
        {
            Customer = editedCustomer;
            return Succeeds;
        }
    }
}
