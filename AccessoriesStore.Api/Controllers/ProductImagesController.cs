using AccessoriesStore.Api.DTOs.Products;
using AccessoriesStore.Application.Abstractions.Products;
using AccessoriesStore.Application.Common.Responses;
using AccessoriesStore.Application.DTOs.Products;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccessoriesStore.Api.Controllers
{
    [ApiController]
    [Route("api/products/{productId:int}/images")]
    public class ProductImagesController : ControllerBase
    {
        private readonly IProductImageService _productImageService;
        private readonly IValidator<UploadProductImageRequest> _uploadProductImageValidator;
        public ProductImagesController(
            IProductImageService productImageService,
            IValidator<UploadProductImageRequest> uploadProductImageValidator)
        {
            _productImageService = productImageService;
            _uploadProductImageValidator = uploadProductImageValidator;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Upload(
            int productId,
            [FromForm] UploadProductImageForm form)
        {
            await using var stream = form.File.OpenReadStream();

            var request = new UploadProductImageRequest
            {
                File = stream,
                FileName = form.File.FileName,
                ContentType = form.File.ContentType
            };

            var validationResult =
                await _uploadProductImageValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(
                    " | ",
                    validationResult.Errors.Select(e => e.ErrorMessage));

                return BadRequest(
                    ApiResponse<object>.Failure(errors)
                );
            }

            var result = await _productImageService.UploadAsync(
                productId,
                request);

            return Ok(
                ApiResponse<ProductImageResponse>.SuccessResponse(
                    result,
                    "Product image uploaded successfully.")
            );
        }

        [HttpGet]
        public async Task<IActionResult> GetByProductId(int productId)
        {
            var result = await _productImageService
                .GetByProductIdAsync(productId);

            return Ok(
                    ApiResponse<List<ProductImageResponse>>.SuccessResponse(result)
                );
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{imageId:int}/main")]
        public async Task<IActionResult> SetMainImage(int productId,int imageId)
        {
            await _productImageService.SetMainImageAsync(
                productId,
                imageId);

            return Ok(
                ApiResponse<object>.SuccessResponse(
                    null,
                    "Product image set as main successfully.")
            );
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{imageId:int}")]
        public async Task<IActionResult> Delete(int productId,int imageId)
        {
            await _productImageService.DeleteAsync(
                productId,
                imageId);

            return Ok(
                ApiResponse<object>.SuccessResponse(
                    null,
                    "Product image deleted successfully.")
            );
        }
    }
}
