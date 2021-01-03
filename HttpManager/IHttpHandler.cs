using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HttpManager
{
    public interface IHttpHandler
    {
        Task<HttpClient> GetClient(string urlKey, string clientKey, string scopeKey);
    }
}
