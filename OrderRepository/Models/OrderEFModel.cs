using System;
using System.Collections.Generic;
using System.Text;

namespace Order.Repository.Models
{
    public class OrderEFModel
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }

        public double Total { get; set; }
    }
}
