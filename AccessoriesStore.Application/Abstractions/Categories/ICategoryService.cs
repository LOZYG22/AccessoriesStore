using AccessoriesStore.Application.DTOs.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.Abstractions.Categories
{
    public interface ICategoryService
    {
        Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);

        Task<CategoryResponse> GetByIdAsync(int id);

        Task<List<CategoryResponse>> GetAllAsync();

        Task<CategoryResponse> UpdateAsync(
            int id,
            UpdateCategoryRequest request);

        Task DeleteAsync(int id);
    }
}
