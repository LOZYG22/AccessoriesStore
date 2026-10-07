using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.DTOs.Orders
{
    public class ConfirmOrderRequest
    {
        public decimal ShippingCost { get; set; }
        public decimal Discount { get; set; }
    }
}
