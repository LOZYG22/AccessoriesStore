using AccessoriesStore.Application.DTOs.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.Abstractions.Products
{
    public interface IProductImageService
    {
        Task<ProductImageResponse> UploadAsync(int productId,UploadProductImageRequest request);
        Task<List<ProductImageResponse>> GetByProductIdAsync(int productId);
        Task SetMainImageAsync(int productId, int imageId);
        Task DeleteAsync(int productId, int imageId);
    }
}
