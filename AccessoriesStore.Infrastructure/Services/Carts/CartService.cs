using AccessoriesStore.Application.Abstractions.Carts;
using AccessoriesStore.Application.DTOs.Carts;
using AccessoriesStore.Domain.Entities;
using AccessoriesStore.Domain.Exceptions;
using AccessoriesStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessoriesStore.Infrastructure.Services.Carts
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CartResponse> GetCartAsync(string userId)
        {
            var cart = await _context.Carts
                .AsNoTracking()
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .ThenInclude(x => x.Images)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (cart is null)
            {
                return new CartResponse();
            }

            return new CartResponse
            {
                Id = cart.Id,

                Items = cart.Items.Select(x =>
                {
                    var unitPrice = x.Product.DiscountPrice ?? x.Product.Price;

                    var mainImage = x.Product.Images
                        .FirstOrDefault(i => i.IsMain);

                    return new CartItemResponse
                    {
                        ProductId = x.ProductId,
                        ProductName = x.Product.Name,
                        ImageUrl = mainImage?.ImageUrl,
                        UnitPrice = unitPrice,
                        Quantity = x.Quantity,
                        TotalPrice = unitPrice * x.Quantity
                    };
                }).ToList(),

                SubTotal = cart.Items.Sum(x =>
                    (x.Product.DiscountPrice ?? x.Product.Price) * x.Quantity)
            };
        }

        public async Task<CartResponse> AddToCartAsync(string userId,
            int productId,
            int quantity)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(x =>
                    x.Id == productId &&
                    x.IsActive);

            if (product is null)
                throw new NotFoundException("Product not found.");

            if (quantity <= 0)
                throw new BadRequestException("Quantity must be greater than zero.");

            var cart = await _context.Carts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (cart is null)
            {
                cart = new Cart
                {
                    UserId = userId
                };

                _context.Carts.Add(cart);
            }

            var cartItem = cart.Items
                .FirstOrDefault(x => x.ProductId == productId);

            var currentQuantity = cartItem?.Quantity ?? 0;
            var newQuantity = currentQuantity + quantity;

            if (newQuantity > product.StockQuantity)
                throw new BadRequestException(
                    $"Only {product.StockQuantity} item(s) are available.");

            if (cartItem is null)
            {
                cartItem = new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity
                };

                cart.Items.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetCartAsync(userId);
        }

        public async Task<CartResponse> UpdateQuantityAsync(string userId,
            int productId,
            int quantity)
        {
            if (quantity <= 0)
                throw new BadRequestException("Quantity must be greater than zero.");

            var cart = await _context.Carts
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (cart is null)
                throw new NotFoundException("Cart not found.");

            var cartItem = cart.Items
                .FirstOrDefault(x => x.ProductId == productId);

            if (cartItem is null)
                throw new BadRequestException("Product is not in the cart.");

            if (!cartItem.Product.IsActive)
                throw new BadRequestException(
                    "Product is no longer available.");

            if (quantity > cartItem.Product.StockQuantity)
                throw new BadRequestException(
                    $"Only {cartItem.Product.StockQuantity} item(s) are available.");

            cartItem.Quantity = quantity;
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetCartAsync(userId);
        }

        public async Task RemoveFromCartAsync(
            string userId,
            int productId)
        {
            var cart = await _context.Carts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (cart is null)
                throw new NotFoundException("Cart not found.");

            var cartItem = cart.Items
                .FirstOrDefault(x => x.ProductId == productId);

            if (cartItem is null)
                throw new BadRequestException("Product is not in the cart.");

            _context.CartItems.Remove(cartItem);

            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (cart is null)
                throw new NotFoundException("Cart not found.");

            _context.CartItems.RemoveRange(cart.Items);

            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
