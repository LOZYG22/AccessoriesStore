using AccessoriesStore.Application.Abstractions.Carts;
using AccessoriesStore.Application.Common.Responses;
using AccessoriesStore.Application.DTOs.Carts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AccessoriesStore.Api.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        public CartController(
            ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _cartService.GetCartAsync(userId!);

            return Ok(
                    ApiResponse<CartResponse>.SuccessResponse(result)
                );
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddToCart(AddToCartRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _cartService.AddToCartAsync(
                userId!,
                request.ProductId,
                request.Quantity);

            return Ok(
                    ApiResponse<CartResponse>.SuccessResponse(
                        result,
                        "Product added to cart successfully.")
                );
        }

        [HttpPut("items/{productId:int}")]
        public async Task<IActionResult> UpdateQuantity(
            int productId,
            UpdateCartItemRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _cartService.UpdateQuantityAsync(
                userId!,
                productId,
                request.Quantity);

            return Ok(
                    ApiResponse<CartResponse>.SuccessResponse(
                        result,
                        "Cart item updated successfully.")
                );
        }

        [HttpDelete("items/{productId:int}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _cartService.RemoveFromCartAsync(
                userId!,
                productId);

            return Ok(
                    ApiResponse<object>.SuccessResponse(
                        null,
                        "Product removed from cart successfully.")
                );
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _cartService.ClearCartAsync(userId!);

            return Ok(
                    ApiResponse<object>.SuccessResponse(
                        null,
                        "Cart cleared successfully.")
                );
        }
    }
}
