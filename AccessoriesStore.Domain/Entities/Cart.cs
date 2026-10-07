using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Domain.Entities
{
    public class Cart
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
