using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.DTOs.Carts
{
    public class CartResponse
    {
        public int Id { get; set; }

        public List<CartItemResponse> Items { get; set; } = new();

        public decimal SubTotal { get; set; }
    }
}
