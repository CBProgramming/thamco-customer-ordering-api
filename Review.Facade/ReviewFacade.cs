using HttpManager;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Review.Facade.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Review.Facade
{
    public class ReviewFacade : IReviewFacade
    {
        private readonly IConfiguration _config;
        private readonly IHttpHandler _handler;

        public ReviewFacade(IConfiguration config, IHttpHandler handler)
        {
            _config = config;
            _handler = handler;
        }

        public async Task<bool> NewPurchases(PurchaseDto purchases)
        {
            if (purchases == null || purchases.OrderedItems.Count == 0)
            {
                return false;
            }
            HttpClient httpClient = await _handler.GetClient("CustomerAuthServerUrl", "ReviewAPI", "ReviewScope");
            if (httpClient != null)
            {
                string uri = _config.GetSection("ReviewProductUri").Value;
                if ((await httpClient.PostAsJsonAsync<PurchaseDto>(uri, purchases)).IsSuccessStatusCode)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
