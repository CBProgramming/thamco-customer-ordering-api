﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Order.Repository.Data
{
    public class ProductEFModel
    {
        public int ProductId { get; set; }

        public string Name { get; set; }

        public int Quantity { get; set; }
    }
}
