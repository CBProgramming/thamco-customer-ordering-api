using HttpManager;
using IdentityModel.Client;
using Invoicing.Facade.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Invoicing.Facade
{
    public class InvoiceFacade : IInvoiceFacade
    {
        private readonly IConfiguration _config;
        private readonly IHttpHandler _handler;

        public InvoiceFacade(IConfiguration config, IHttpHandler handler)
        {
            _config = config;
            _handler = handler;
        }

        public async Task<bool> NewOrder(OrderInvoiceDto order)
        {
            if (order == null || order.OrderedItems.Count == 0)
            {
                return false;
            }
            HttpClient httpClient = await _handler.GetClient("CustomerAuthServerUrl", "InvoiceAPI", "InvoiceScope");
            if (httpClient != null)
            {
                string uri = _config.GetSection("InvoiceUri").Value;
                if ((await httpClient.PostAsJsonAsync<OrderInvoiceDto>(uri, order)).IsSuccessStatusCode)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
