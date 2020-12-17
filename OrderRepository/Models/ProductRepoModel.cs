using System;
using System.Collections.Generic;
using System.Text;

namespace Order.Repository.Data
{
    public class ProductRepoModel
    {
        public int ProductId { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }

        public int Quantity { get; set; }
    }
}
