using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.DTOs.Categories
{
    public class UpdateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
