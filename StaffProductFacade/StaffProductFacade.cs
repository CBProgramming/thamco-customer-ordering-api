using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using StaffProduct.Facade.Models;
using System.Net.Http;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using HttpManager;

namespace StaffProduct.Facade
{
    public class StaffProductFacade : IStaffProductFacade
    {
        private readonly IHttpHandler _handler;
        private string staffAuthUrl;
        private string staffProductApi;
        private string staffProductScope;
        private string staffProductUri;

        public StaffProductFacade(IConfiguration config, IHttpHandler handler)
        {
            _handler = handler;
            if (config != null)
            {
                staffAuthUrl = config.GetSection("StaffAuthServerUrlKey").Value;
                staffProductApi = config.GetSection("StaffProductAPIKey").Value;
                staffProductScope = config.GetSection("StaffProductScopeKey").Value;
                staffProductUri = config.GetSection("StaffProductUri").Value;
            }
        }

        public async Task<bool> UpdateStock(List<StockReductionDto> stockReductions)
        {
            if (stockReductions == null || stockReductions.Count == 0 || !ValidConfigStrings())
            {
                return false;
            }

            HttpClient httpClient = await _handler.GetClient(staffAuthUrl, staffProductApi, staffProductScope);
            if (httpClient != null)
            {
                if ((await httpClient.PutAsJsonAsync<List<StockReductionDto>>(staffProductUri, stockReductions)).IsSuccessStatusCode)
                {
                    return true;
                }
            }
            return false;
        }

        private bool ValidConfigStrings()
        {
            return !string.IsNullOrEmpty(staffAuthUrl)
                    && !string.IsNullOrEmpty(staffProductApi)
                    && !string.IsNullOrEmpty(staffProductScope)
                    && !string.IsNullOrEmpty(staffProductUri);
        }
    }
}
