using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerOrderingService.Models
{
    public class OrderDto
    {
        public int CustomerId { get; set; }

        public int OrderId { get; set; }

        public DateTime OrderDate { get; set; }

        public double Total { get; set; }

        public List<OrderedItemDto> Products { get; set; }
    }
}
