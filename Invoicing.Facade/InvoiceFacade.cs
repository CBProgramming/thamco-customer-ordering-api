using IdentityModel.Client;
using Invoicing.Facade.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Invoicing.Facade
{
    public class InvoiceFacade : IInvoiceFacade
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public InvoiceFacade(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        private async Task<HttpClient> GetClientWithAccessToken()
        {
            var client = _httpClientFactory.CreateClient("InvoiceAPI");
            string authServerUrl = _config.GetSection("StaffAuthServerUrl").Value;
            string clientSecret = _config.GetSection("ClientSecret").Value;
            string clientId = _config.GetSection("ClientId").Value;
            var disco = await client.GetDiscoveryDocumentAsync(authServerUrl);
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = "invoice_api"
            });
            client.SetBearerToken(tokenResponse.AccessToken);
            return client;
        }

        public async Task<bool> NewOrder(OrderInvoiceDto order)
        {
            if (order == null || order.OrderedItems.Count == 0)
            {
                return false;
            }
            HttpClient httpClient = await GetClientWithAccessToken();
            string uri = _config.GetSection("InvoiceUri").Value;
            if ((await httpClient.PostAsJsonAsync<OrderInvoiceDto>(uri, order)).IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
    }
}
