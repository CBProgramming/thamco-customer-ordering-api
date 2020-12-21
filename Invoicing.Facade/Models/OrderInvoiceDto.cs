using System;
using System.Collections.Generic;
using System.Text;

namespace Invoicing.Facade.Models
{
    public class OrderInvoiceDto
    {
        public int CustomerId { get; set; }

        public int OrderId { get; set; }

        public DateTime OrderDate { get; set; }

        public double Total { get; set; }

        public List<InvoiceItemDto> Products { get; set; }
    }
}
