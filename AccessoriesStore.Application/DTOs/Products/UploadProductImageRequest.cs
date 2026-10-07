using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.DTOs.Products
{
    public class UploadProductImageRequest
    {
        public Stream File { get; set; } = Stream.Null;

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;
    }
}
