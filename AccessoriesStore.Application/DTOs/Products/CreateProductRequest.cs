using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.DTOs.Products
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }

        public string SKU { get; set; } = string.Empty;
        public int StockQuantity { get; set; }

        public int CategoryId { get; set; }

        public bool IsFeatured { get; set; }
    }
}
