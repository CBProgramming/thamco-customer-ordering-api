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
        private readonly IHttpHandler _handler;
        private string staffAuthUrl;
        private string invoiceApi;
        private string invoiceScope;
        private string invoiceUri;

        public InvoiceFacade(IConfiguration config, IHttpHandler handler)
        {
            _handler = handler;
            if (config != null)
            {
                staffAuthUrl = config.GetSection("StaffAuthServerUrlKey").Value;
                invoiceApi = config.GetSection("InvoiceAPIKey").Value;
                invoiceScope = config.GetSection("InvoiceScopeKey").Value;
                invoiceUri = config.GetSection("InvoiceUri").Value;
            }
        }

        public async Task<bool> NewOrder(OrderInvoiceDto order)
        {
            if (order == null || order.OrderedItems.Count == 0 || !ValidConfigStrings())
            {
                return false;
            }
            HttpClient httpClient = await _handler.GetClient(staffAuthUrl, invoiceApi, invoiceScope);
            if (httpClient != null)
            {
                if ((await httpClient.PostAsJsonAsync<OrderInvoiceDto>(invoiceUri, order)).IsSuccessStatusCode)
                {
                    return true;
                }
            }
            return false;
        }

        private bool ValidConfigStrings()
        {
            return !string.IsNullOrEmpty(staffAuthUrl)
                    && !string.IsNullOrEmpty(invoiceApi)
                    && !string.IsNullOrEmpty(invoiceScope)
                    && !string.IsNullOrEmpty(invoiceUri);
        }
    }
}
