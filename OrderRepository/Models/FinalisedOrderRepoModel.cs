using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Order.Repository.Models
{
    public class FinalisedOrderRepoModel
    {
        public int CustomerId { get; set; }

        public DateTime OrderDate { get; set; }

        public List<OrderedItemRepoModel> OrderedItems { get; set; }

        [Range(0,double.MaxValue)]
        public double Total { get; set; }

        public int OrderId { get; set; }
    }
}
