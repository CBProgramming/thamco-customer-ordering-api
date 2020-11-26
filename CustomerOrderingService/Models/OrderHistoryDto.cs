using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerOrderingService.Models
{
    public class OrderHistoryDto
    {
        public int CustomerId { get; set; }

        public int OrderId { get; set; }

        public double Total { get; set; }

        public DateTime Date { get; set; }
    }
}
