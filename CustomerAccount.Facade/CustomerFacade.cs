using CustomerAccount.Facade.Models;
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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public CustomerFacade(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<bool> DeleteCustomer(int customerId)
        {
            HttpClient httpClient = await GetClientWithAccessToken();
            string uri = "/api/Customer/" + customerId;
            if ((await httpClient.DeleteAsync(uri)).IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> EditCustomer(CustomerFacadeDto customer)
        {
            if (customer != null)
            {
                HttpClient httpClient = await GetClientWithAccessToken();
                if (customer != null)
                {
                    string uri = "/api/Customer/" + customer.CustomerId;
                    if ((await httpClient.PutAsJsonAsync<CustomerFacadeDto>(uri, customer)).IsSuccessStatusCode)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private async Task<HttpClient> GetClientWithAccessToken()
        {
            var client = _httpClientFactory.CreateClient("CustomerAccountAPI");
            string authServerUrl = _config.GetConnectionString("CustomerAuthServerUrl");
            string clientSecret = _config.GetConnectionString("ClientSecret");
            string clientId = _config.GetConnectionString("ClientId");
            var disco = await client.GetDiscoveryDocumentAsync(authServerUrl);
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = "customer_account_api"
            });
            client.SetBearerToken(tokenResponse.AccessToken);
            return client;
        }
    }
}
