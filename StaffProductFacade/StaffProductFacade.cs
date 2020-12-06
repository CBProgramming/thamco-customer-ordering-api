using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using StaffProduct.Facade.Models;
using System.Net.Http;
using IdentityModel.Client;

namespace StaffProduct.Facade
{
    public class StaffProductFacade : IStaffProductFacade
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public StaffProductFacade(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> UpdateStock(List<StockReductionDto> stockReductions)
        {
            var httpClient = _httpClientFactory.CreateClient("StaffProductAPI");
            httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
            });
            string uri = "/api/Products/";
            if((await httpClient.PostAsJsonAsync<List<StockReductionDto>>(uri,stockReductions)).IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
    }
}
