﻿using System.Collections.Generic;

namespace ASI.Contracts.Excit.Additional
{
    public class Product
    {
        public string Number { get; set; }
        public List<string> Numbers { get; set; }
        public List<ProductSku> SKU { get; set; }
    }
}
