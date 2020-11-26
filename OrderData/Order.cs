using System;
using System.Collections.Generic;
using System.Text;

namespace OrderData
{
    public class Order
    {
        public int OrderId {get; set;}

        public DateTime OrderDate { get; set; }

        public double Total { get; set; }

        public virtual IList<OrderedItem> OrderedItems { get; set; }

        public int CustomerId { get; set; }

        public Customer Customer { get; set; }
    }
}
