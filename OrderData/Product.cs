﻿using System;
using System.Collections.Generic;

namespace OrderData
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Value { get; set; }

        public virtual IList<BasketItem> BasketItems { get; set; }

        public virtual IList<OrderedItem> OrderedItems { get; set; }
    }
}
