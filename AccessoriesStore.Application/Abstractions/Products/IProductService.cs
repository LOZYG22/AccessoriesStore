using AccessoriesStore.Application.DTOs.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.Abstractions.Products
{
    public interface IProductService
    {
        Task<ProductResponse> CreateAsync(CreateProductRequest request);

        Task<ProductResponse> GetByIdAsync(int id);

        Task<ProductResponse> GetBySlugAsync(string slug);

        Task<ProductPagedResponse> GetAllAsync(ProductFilterRequest request);
        Task<ProductResponse> UpdateAsync(
            int id,
            UpdateProductRequest request);

        Task DeleteAsync(int id);

        Task<List<ProductResponse>> GetAllForAdminAsync();
    }
}
