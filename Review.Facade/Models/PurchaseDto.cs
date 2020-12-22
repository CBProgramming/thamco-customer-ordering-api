using System;
using System.Collections.Generic;
using System.Text;

namespace Review.Facade.Models
{
    public class PurchaseDto
    {
        public int CustomerId { get; set; }

        public string CustomerAuthId { get; set; }

        public List<ProductDto> OrderedItems { get; set; }
    }
}
