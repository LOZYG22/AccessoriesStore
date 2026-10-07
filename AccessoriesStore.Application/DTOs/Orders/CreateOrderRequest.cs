using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.DTOs.Orders
{
    public class CreateOrderRequest
    {
        public int AddressId { get; set; }
        public string? Notes { get; set; }
    }
}
