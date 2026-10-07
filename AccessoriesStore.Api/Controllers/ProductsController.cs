using AccessoriesStore.Application.Abstractions.Products;
using AccessoriesStore.Application.Common.Responses;
using AccessoriesStore.Application.DTOs.Products;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccessoriesStore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(
            IProductService productService)
        {
            _productService = productService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductRequest request)
        {
            var result = await _productService.CreateAsync(request);

            return Ok(
                    ApiResponse<ProductResponse>.SuccessResponse(
                        result,
                        "Product created successfully.")
                );
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _productService.GetByIdAsync(id);

            return Ok(
                ApiResponse<ProductResponse>.SuccessResponse(result)
            );
        }

        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var result = await _productService.GetBySlugAsync(slug);

            return Ok(
                ApiResponse<ProductResponse>.SuccessResponse(result)
            );
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<ProductPagedResponse>>>
           GetAll([FromQuery] ProductFilterRequest request)
        {
            var products = await _productService.GetAllAsync(request);

            return Ok(
                ApiResponse<ProductPagedResponse>
                    .SuccessResponse(products));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<IActionResult> GetAllForAdmin()
        {
            var result = await _productService.GetAllForAdminAsync();

            return Ok(
                ApiResponse<List<ProductResponse>>.SuccessResponse(result)
            );
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            UpdateProductRequest request)
        {
            var result = await _productService.UpdateAsync(id, request);

            return Ok(
                    ApiResponse<ProductResponse>.SuccessResponse(
                        result,
                        "Product updated successfully.")
                );
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);

            return Ok(
                    ApiResponse<object>.SuccessResponse(
                        null,
                        "Product deactivated successfully.")
                );
        }
    }
}
