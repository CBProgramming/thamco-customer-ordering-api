using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerOrderingService.Models
{
    public class OrderHistoryDto
    {
        public CustomerDto Customer { get; set; }

        public List<OrderDto> Orders { get; set; }
    }
}
