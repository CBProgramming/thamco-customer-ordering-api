using Invoicing.Facade.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Invoicing.Facade
{
    public class FakeInvoiceFacade : IInvoiceFacade
    {
        bool Succeeds = true;

        public async Task<bool> NewOrder(OrderInvoiceDto order)
        {
            return Succeeds;
        }
    }
}
