using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.DTOs.Images
{
    public class ImageUploadResult
    {
        public string ImageUrl { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
    }
}
