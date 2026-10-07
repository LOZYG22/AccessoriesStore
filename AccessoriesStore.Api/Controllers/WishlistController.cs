using AccessoriesStore.Application.Abstractions.Wishlist;
using AccessoriesStore.Application.Common.Responses;
using AccessoriesStore.Application.DTOs.Wishlist;
using AccessoriesStore.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AccessoriesStore.Api.Controllers;

[ApiController]
[Route("api/wishlist")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<WishlistItemResponse>>> Add(
        AddToWishlistRequest request)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedException(
                "User is not authenticated.");
        }

        var item = await _wishlistService.AddAsync(
            userId,
            request);

        return Ok(
            ApiResponse<WishlistItemResponse>.SuccessResponse(
                item,
                "Product added to wishlist successfully."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<WishlistItemResponse>>>> GetMyWishlist()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedException(
                "User is not authenticated.");
        }

        var items = await _wishlistService
            .GetMyWishlistAsync(userId);

        return Ok(
            ApiResponse<IEnumerable<WishlistItemResponse>>
                .SuccessResponse(items));
    }

    [HttpDelete("{wishlistItemId}")]
    public async Task<ActionResult<ApiResponse<object>>> Remove(int wishlistItemId)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedException(
                "User is not authenticated.");
        }

        await _wishlistService.RemoveAsync(
            userId,
            wishlistItemId);

        return Ok(
            ApiResponse<object>.SuccessResponse(
                null,
                "Product removed from wishlist successfully."));
    }
}