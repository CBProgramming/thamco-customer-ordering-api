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
        private readonly IConfiguration _config;
        private readonly IHttpHandler _handler;

        public StaffProductFacade(IConfiguration config, IHttpHandler handler)
        {
            _config = config;
            _handler = handler;
        }

        public async Task<bool> UpdateStock(List<StockReductionDto> stockReductions)
        {
            if (stockReductions == null || stockReductions.Count == 0)
            {
                return false;
            }

            HttpClient httpClient = await _handler.GetClient("CustomerAuthServerUrl", "StaffProductAPI", "StaffProductScope");
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
