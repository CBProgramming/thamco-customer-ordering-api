using CustomerAccount.Facade.Models;
using HttpManager;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
namespace CustomerAccount.Facade
{
    public class CustomerFacade : ICustomerAccountFacade
    {
        private readonly IHttpHandler _handler;
        private string customerAuthUrl;
        private string customerApi;
        private string customerScope;
        private string customerUri;

        public CustomerFacade(IConfiguration config, IHttpHandler handler)
        {
            _handler = handler;
            if (config != null)
            {
                customerAuthUrl = config.GetSection("CustomerAuthServerUrlKey").Value;
                customerApi = config.GetSection("CustomerAccountAPIKey").Value;
                customerScope = config.GetSection("CustomerAccountScopeKey").Value;
                customerUri = config.GetSection("CustomerUri").Value;
            }
        }

        public async Task<bool> DeleteCustomer(int customerId)
        {
            if (!ValidConfigStrings())
            {
                return false;
            }
            HttpClient httpClient = await _handler.GetClient(customerAuthUrl,
                customerApi, customerScope);
            if (httpClient == null)
            {
                return false;
            }
            string uri = customerUri + customerId;
            if ((await httpClient.DeleteAsync(uri)).IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> EditCustomer(CustomerFacadeDto customer)
        {
            if (ValidConfigStrings() &&customer != null)
            {
                HttpClient httpClient = await _handler.GetClient(customerAuthUrl,
                customerApi, customerScope);
                if (httpClient == null)
                {
                    return false;
                }
                string uri = customerUri + customer.CustomerId;
                if ((await httpClient.PutAsJsonAsync<CustomerFacadeDto>(uri, customer)).IsSuccessStatusCode)
                {
                    return true;
                }
            }
            return false;
        }

        private bool ValidConfigStrings()
        {
            return !string.IsNullOrEmpty(customerAuthUrl)
                    && !string.IsNullOrEmpty(customerApi)
                    && !string.IsNullOrEmpty(customerScope)
                    && !string.IsNullOrEmpty(customerUri);
        }
    }
}
