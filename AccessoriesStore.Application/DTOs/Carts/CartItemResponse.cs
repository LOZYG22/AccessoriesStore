using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.DTOs.Carts
{
    public class CartItemResponse
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
