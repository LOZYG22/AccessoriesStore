using AccessoriesStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public int AddressId { get; set; }
        public string ShippingFullName { get; set; } = string.Empty;
        public string ShippingPhoneNumber { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingArea { get; set; } = string.Empty;
        public string ShippingStreet { get; set; } = string.Empty;
        public string? ShippingBuildingNumber { get; set; }
        public string? ShippingApartmentNumber { get; set; }
        public string? ShippingAdditionalDetails { get; set; }

        public decimal SubTotal { get; set; }

        public decimal ShippingCost { get; set; }

        public decimal Discount { get; set; }

        public decimal Total { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        public string? PaymentIntentId { get; set; }
        public long? PaymobOrderId { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
