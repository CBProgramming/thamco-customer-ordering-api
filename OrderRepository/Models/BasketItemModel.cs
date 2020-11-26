using System;
using System.Collections.Generic;
using System.Text;

namespace Order.Repository.Models
{
    public class BasketItemModel
    {
        public int CustomerId { get; set; }
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public double Price { get; set; }

        public int Quantity { get; set; }
    }
}
