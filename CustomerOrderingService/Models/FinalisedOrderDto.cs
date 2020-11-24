using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerOrderingService.Models
{
    public class FinalisedOrderDto
    {
        public int CustomerId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public List<OrderedItemDto> OrderedItems {get; set;}

        public double Total { get; set; }
    }
}
