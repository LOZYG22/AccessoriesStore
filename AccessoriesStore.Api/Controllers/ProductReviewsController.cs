using AccessoriesStore.Application.Abstractions.ProductReviews;
using AccessoriesStore.Application.Common.Responses;
using AccessoriesStore.Application.DTOs.ProductReviews;
using AccessoriesStore.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AccessoriesStore.Api.Controllers;

[ApiController]
[Route("api")]
public class ProductReviewsController : ControllerBase
{
    private readonly IProductReviewService _reviewService;

    public ProductReviewsController(
        IProductReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [Authorize]
    [HttpPost("products/{productId}/reviews")]
    public async Task<ActionResult<ApiResponse<ProductReviewResponse>>> Create(
        int productId,
        CreateProductReviewRequest request)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedException(
                "User is not authenticated.");
        }

        var review = await _reviewService.CreateAsync(
            userId,
            productId,
            request);

        return Ok(
            ApiResponse<ProductReviewResponse>.SuccessResponse(
                review,
                "Review created successfully."));
    }

    [Authorize]
    [HttpPut("reviews/{reviewId}")]
    public async Task<ActionResult<ApiResponse<ProductReviewResponse>>> Update(
        int reviewId,
        CreateProductReviewRequest request)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedException(
                "User is not authenticated.");
        }

        var review = await _reviewService.UpdateAsync(
            userId,
            reviewId,
            request);

        return Ok(
            ApiResponse<ProductReviewResponse>.SuccessResponse(
                review,
                "Review updated successfully."));
    }

    [Authorize]
    [HttpDelete("reviews/{reviewId}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        int reviewId)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedException(
                "User is not authenticated.");
        }

        await _reviewService.DeleteAsync(
            userId,
            reviewId);

        return Ok(
            ApiResponse<object>.SuccessResponse(
                null,
                "Review deleted successfully."));
    }

    [AllowAnonymous]
    [HttpGet("products/{productId}/reviews")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductReviewResponse>>>> GetByProduct(
        int productId)
    {
        var reviews = await _reviewService
            .GetByProductIdAsync(productId);

        return Ok(
            ApiResponse<IEnumerable<ProductReviewResponse>>
                .SuccessResponse(reviews));
    }
}