using System;
using System.Collections.Generic;
using System.Text;

namespace Invoicing.Facade.Models
{
    public class PaulsDto
    {
        public int CustomerId { get; set; }

        public int OrderId { get; set; }

        public DateTime DateTime { get; set; }

        public double TotalPrice { get; set; }

        public List<InvoiceItemDto> Products { get; set; }
    }
}
