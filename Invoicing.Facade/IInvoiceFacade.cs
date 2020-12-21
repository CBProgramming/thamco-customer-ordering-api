using Invoicing.Facade.Models;
using System;
using System.Threading.Tasks;

namespace Invoicing.Facade
{
    public interface IInvoiceFacade
    {
        public Task<bool> NewOrder(OrderInvoiceDto order);
    }
}
