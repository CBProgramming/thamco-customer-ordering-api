using Invoicing.Facade.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Invoicing.Facade
{
    public class InvoiceFacade : IInvoiceFacade
    {
        public Task<bool> NewOrder(OrderInvoiceDto order)
        {
            throw new NotImplementedException();
        }
    }
}
