using System;
using System.Collections.Generic;
using System.Text;

namespace Order.Repository.Models
{
    public class OrderedItemEFModel
    {
        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public double Price { get; set; }

        public string Name { get; set; }
    }
}
