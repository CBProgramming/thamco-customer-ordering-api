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

        public async Task<bool> DeleteCustomer(int customerId)
        {
            return Succeeds;
        }

        public async Task<bool> EditCustomer(CustomerFacadeDto editedCustomer)
        {
            return Succeeds;
        }
    }
}
