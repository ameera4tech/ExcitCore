using System;
using System.Collections.Generic;
using System.Text;

namespace ASI.Contracts.Excit.Additional
{
    public class ProductSku
    {
        public string SKU { get; set; }
        public List<ProductSkuValue> Values { get; set; }
    }
}
