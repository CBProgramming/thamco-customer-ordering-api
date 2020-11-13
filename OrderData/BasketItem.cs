using System;
using System.Collections.Generic;
using System.Text;

namespace OrderData
{
    public class BasketItem
    {
        public int CustomerId { get; set; }

        public Customer Customer { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; }

        public int Quantity { get; set; }




    }
}
