using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HttpManager
{
    public interface IAccessTokenGetter
    {
        Task<HttpClient> GetToken(HttpClient client, string authUrl, string clientId, string clientSecret, string scopeKey);
    }
}