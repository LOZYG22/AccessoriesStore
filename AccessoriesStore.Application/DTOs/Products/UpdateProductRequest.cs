using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.DTOs.Products
{
    public class UpdateProductRequest
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }

        public string SKU { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public bool IsActive { get; set; }

        public bool IsFeatured { get; set; }
    }
}
