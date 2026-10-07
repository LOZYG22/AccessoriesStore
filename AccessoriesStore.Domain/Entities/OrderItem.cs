using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal TotalPrice { get; set; }

        public Order Order { get; set; } = null!;

        public Product Product { get; set; } = null!;
    }
}
