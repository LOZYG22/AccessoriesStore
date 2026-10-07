using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Domain.Entities
{
    public class ProductImage
    {
        public int Id { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public bool IsMain { get; set; }

        public int DisplayOrder { get; set; }

        public int ProductId { get; set; }
        public string PublicId { get; set; } = string.Empty;

        public Product Product { get; set; } = null!;
    }
}
