using AccessoriesStore.Application.DTOs.Addresses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.DTOs.Orders
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public AddressResponse Address { get; set; } = null!;
        public List<OrderItemResponse> Items { get; set; } = new();
    }
}
