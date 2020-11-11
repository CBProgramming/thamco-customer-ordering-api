using System;
using System.Collections.Generic;
using System.Text;

namespace OrderRepository.Data
{
    public class ProductEFModel
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }
    }
}
