using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Order.Repository.Models
{
    public class FinalisedOrderEFModel
    {
        public int CustomerId { get; set; }

        public DateTime OrderDate { get; set; }

        public List<OrderedItemEFModel> OrderedItems { get; set; }

        [Range(0,double.MaxValue)]
        public double Total { get; set; }
    }
}
