using AccessoriesStore.Application.Abstractions.Wishlist;
using AccessoriesStore.Application.DTOs.Wishlist;
using AccessoriesStore.Domain.Entities;
using AccessoriesStore.Domain.Exceptions;
using AccessoriesStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessoriesStore.Infrastructure.Services.Wishlist;

public class WishlistService : IWishlistService
{
    private readonly ApplicationDbContext _context;

    public WishlistService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WishlistItemResponse> AddAsync(
        string userId,
        AddToWishlistRequest request)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p =>
                p.Id == request.ProductId &&
                p.IsActive);

        if (product is null)
        {
            throw new NotFoundException(
                "Product not found.");
        }

        var existingItem = await _context.WishlistItems
            .AnyAsync(w =>
                w.UserId == userId &&
                w.ProductId == request.ProductId);

        if (existingItem)
        {
            throw new BadRequestException(
                "Product is already in your wishlist.");
        }

        var wishlistItem = new WishlistItem
        {
            UserId = userId,
            ProductId = request.ProductId
        };

        _context.WishlistItems.Add(wishlistItem);

        await _context.SaveChangesAsync();

        return await GetItemResponseAsync(
            wishlistItem.Id);
    }

    public async Task RemoveAsync(
        string userId,
        int wishlistItemId)
    {
        var wishlistItem = await _context.WishlistItems
            .FirstOrDefaultAsync(w =>
                w.Id == wishlistItemId &&
                w.UserId == userId);

        if (wishlistItem is null)
        {
            throw new NotFoundException(
                "Wishlist item not found.");
        }

        _context.WishlistItems.Remove(wishlistItem);

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<WishlistItemResponse>> GetMyWishlistAsync(
        string userId)
    {
        return await _context.WishlistItems
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new WishlistItemResponse
            {
                Id = w.Id,
                ProductId = w.ProductId,
                ProductName = w.Product.Name,
                ProductSlug = w.Product.Slug,
                Price = w.Product.Price,
                DiscountPrice = w.Product.DiscountPrice,
                MainImageUrl = w.Product.Images
                    .Where(i => i.IsMain)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault(),
                CreatedAt = w.CreatedAt
            })
            .ToListAsync();
    }

    private async Task<WishlistItemResponse> GetItemResponseAsync(
        int wishlistItemId)
    {
        var item = await _context.WishlistItems
            .AsNoTracking()
            .Where(w => w.Id == wishlistItemId)
            .Select(w => new WishlistItemResponse
            {
                Id = w.Id,
                ProductId = w.ProductId,
                ProductName = w.Product.Name,
                ProductSlug = w.Product.Slug,
                Price = w.Product.Price,
                DiscountPrice = w.Product.DiscountPrice,
                MainImageUrl = w.Product.Images
                    .Where(i => i.IsMain)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault(),
                CreatedAt = w.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (item is null)
        {
            throw new NotFoundException(
                "Wishlist item not found.");
        }

        return item;
    }
}