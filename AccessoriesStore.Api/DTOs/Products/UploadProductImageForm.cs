using Microsoft.AspNetCore.Http;

namespace AccessoriesStore.Api.DTOs.Products
{
    public class UploadProductImageForm
    {
        public IFormFile File { get; set; } = null!;
    }
}
