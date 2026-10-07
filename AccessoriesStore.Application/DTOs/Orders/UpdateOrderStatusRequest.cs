using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.DTOs.Orders
{
    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
