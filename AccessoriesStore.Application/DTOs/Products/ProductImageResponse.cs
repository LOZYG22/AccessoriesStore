using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.DTOs.Products
{
    public class ProductImageResponse
    {
        public int Id { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public bool IsMain { get; set; }

        public int DisplayOrder { get; set; }
    }
}
