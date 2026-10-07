using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.DTOs.Categories
{
    public class CategoryResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
