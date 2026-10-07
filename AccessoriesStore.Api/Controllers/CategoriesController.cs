using AccessoriesStore.Application.Abstractions.Categories;
using AccessoriesStore.Application.Common.Responses;
using AccessoriesStore.Application.DTOs.Categories;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AccessoriesStore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryRequest request)
        {
            var result = await _categoryService.CreateAsync(request);

            return Ok(
                    ApiResponse<CategoryResponse>.SuccessResponse(
                        result,
                        "Category created successfully.")
                );
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _categoryService.GetByIdAsync(id);

            return Ok(
                    ApiResponse<CategoryResponse>.SuccessResponse(result)
                );
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAllAsync();

            return Ok(
                    ApiResponse<List<CategoryResponse>>.SuccessResponse(result)
                );
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            UpdateCategoryRequest request)
        {
            var result = await _categoryService.UpdateAsync(id, request);

            return Ok(
                    ApiResponse<CategoryResponse>.SuccessResponse(
                        result,
                        "Category updated successfully.")
                );
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _categoryService.DeleteAsync(id);

            return Ok(
                    ApiResponse<object>.SuccessResponse(
                        null,
                        "Category deleted successfully.")
                );
        }
    }
}
