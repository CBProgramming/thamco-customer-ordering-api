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
        private readonly IHttpHandler _handler;
        private string customerAuthUrl;
        private string reviewApi;
        private string reviewScope;
        private string reviewUri;

        public ReviewFacade(IConfiguration config, IHttpHandler handler)
        {
            _handler = handler;
            if (config != null)
            {
                customerAuthUrl = config.GetSection("CustomerAuthServerUrlKey").Value;
                reviewApi = config.GetSection("ReviewAPIKey").Value;
                reviewScope = config.GetSection("ReviewScopeKey").Value;
                reviewUri = config.GetSection("ReviewUri").Value;
            }
        }

        public async Task<bool> NewPurchases(PurchaseDto purchases)
        {
            if (purchases == null || purchases.OrderedItems.Count == 0 || !ValidConfigStrings())
            {
                return false;
            }
            HttpClient httpClient = await _handler.GetClient("CustomerAuthServerUrl", "ReviewAPI", "ReviewScope");
            if (httpClient != null)
            {
                if ((await httpClient.PostAsJsonAsync<PurchaseDto>(reviewUri, purchases)).IsSuccessStatusCode)
                {
                    return true;
                }
            }
            return false;
        }

        private bool ValidConfigStrings()
        {
            return !string.IsNullOrEmpty(customerAuthUrl)
                    && !string.IsNullOrEmpty(reviewApi)
                    && !string.IsNullOrEmpty(reviewScope)
                    && !string.IsNullOrEmpty(reviewUri);
        }
    }
}
