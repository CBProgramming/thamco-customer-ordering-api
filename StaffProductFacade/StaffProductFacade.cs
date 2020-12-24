using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using StaffProduct.Facade.Models;
using System.Net.Http;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;

namespace StaffProduct.Facade
{
    public class StaffProductFacade : IStaffProductFacade
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public StaffProductFacade(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        private async Task<HttpClient> GetClientWithAccessToken()
        {
            string authServerUrl = _config.GetSection("StaffAuthServerUrl").Value;
            string clientSecret = _config.GetSection("ClientSecret").Value;
            string clientId = _config.GetSection("ClientId").Value;
            if (string.IsNullOrEmpty(authServerUrl) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(clientId))
            {
                return null;
            }
            var client = _httpClientFactory.CreateClient("StaffProductAPI");
            var disco = await client.GetDiscoveryDocumentAsync(authServerUrl);
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = "staff_product_api"
            });
            client.SetBearerToken(tokenResponse.AccessToken);
            return client;
        }

        public async Task<bool> UpdateStock(List<StockReductionDto> stockReductions)
        {
            if (stockReductions == null || stockReductions.Count == 0)
            {
                return false;
            }

            HttpClient httpClient = await GetClientWithAccessToken();
            if (httpClient != null)
            {
                string uri = _config.GetSection("StaffProductUri").Value;
                if ((await httpClient.PostAsJsonAsync<List<StockReductionDto>>(uri, stockReductions)).IsSuccessStatusCode)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
