using System;
using System.Collections.Generic;
using System.Text;

namespace OrderData
{
    public class OrderedItem
    {
        public int OrderId { get; set; }

        public Order Order { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; }

        public int Quantity { get; set; }

        public int Amount { get; set; }

        public string Name { get; set; }
    }
}
