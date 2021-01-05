using Invoicing.Facade.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Invoicing.Facade
{
    public class FakeInvoiceFacade : IInvoiceFacade
    {
        public bool Succeeds = true;
        public OrderInvoiceDto Order;

        public async Task<bool> NewOrder(OrderInvoiceDto order)
        {
            Order = order;
            return Succeeds;
        }
    }
}
