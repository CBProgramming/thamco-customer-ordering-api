using CustomerAccount.Facade.Models;
using System;
using System.Threading.Tasks;

namespace CustomerAccount.Facade
{
    public interface ICustomerAccountFacade
    {
        public Task<bool> EditCustomer(CustomerFacadeDto editedCustomer);

        public Task<bool> DeleteCustomer(int customerId);
    }
}
