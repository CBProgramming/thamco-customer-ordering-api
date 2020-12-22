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
            var client = _httpClientFactory.CreateClient("StaffProductAPI");
            string authServerUrl = _config.GetConnectionString("StaffAuthServerUrl");
            string clientSecret = _config.GetConnectionString("ClientSecret");
            string clientId = _config.GetConnectionString("ClientId");
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
            string uri = _config.GetConnectionString("StaffProductUri");
            if ((await httpClient.PostAsJsonAsync<List<StockReductionDto>>(uri,stockReductions)).IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
    }
}
