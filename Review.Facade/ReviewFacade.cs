﻿using IdentityModel.Client;
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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public ReviewFacade(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<bool> NewPurchases(PurchaseDto purchases)
        {
            if (purchases == null || purchases.OrderedItems.Count == 0)
            {
                return false;
            }
            HttpClient httpClient = await GetClientWithAccessToken();
            string uri = _config.GetConnectionString("ReviewProductUri");
            if ((await httpClient.PostAsJsonAsync<PurchaseDto>(uri, purchases)).IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        private async Task<HttpClient> GetClientWithAccessToken()
        {
            var client = _httpClientFactory.CreateClient("ReviewAPI");
            string authServerUrl = _config.GetConnectionString("CustomerAuthServerUrl");
            string clientSecret = _config.GetConnectionString("ClientSecret");
            string clientId = _config.GetConnectionString("ClientId");
            var disco = await client.GetDiscoveryDocumentAsync(authServerUrl);
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = "review_api"
            });
            client.SetBearerToken(tokenResponse.AccessToken);
            return client;
        }
    }
}
