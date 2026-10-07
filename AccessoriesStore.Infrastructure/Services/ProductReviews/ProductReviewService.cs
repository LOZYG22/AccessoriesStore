using AccessoriesStore.Application.Abstractions.ProductReviews;
using AccessoriesStore.Application.DTOs.ProductReviews;
using AccessoriesStore.Domain.Entities;
using AccessoriesStore.Domain.Enums;
using AccessoriesStore.Domain.Exceptions;
using AccessoriesStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessoriesStore.Infrastructure.Services.ProductReviews;

public class ProductReviewService : IProductReviewService
{
    private readonly ApplicationDbContext _context;

    public ProductReviewService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductReviewResponse> CreateAsync(
        string userId,
        int productId,
        CreateProductReviewRequest request)
    {
        var productExists = await _context.Products
            .AnyAsync(p => p.Id == productId);

        if (!productExists)
        {
            throw new NotFoundException(
                "Product not found.");
        }

        var hasPurchased = await _context.Orders
            .AnyAsync(o =>
                o.UserId == userId &&
                o.Status == OrderStatus.Delivered &&
                o.Items.Any(i => i.ProductId == productId));

        if (!hasPurchased)
        {
            throw new BadRequestException(
                "You can only review products you have purchased.");
        }

        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.FirstName,
                u.LastName
            })
            .FirstOrDefaultAsync();

        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        var existingReview = await _context.ProductReviews
            .AnyAsync(r =>
                r.ProductId == productId &&
                r.UserId == userId);

        if (existingReview)
        {
            throw new BadRequestException(
                "You have already reviewed this product.");
        }

        var review = new ProductReview
        {
            ProductId = productId,
            UserId = userId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        _context.ProductReviews.Add(review);

        await _context.SaveChangesAsync();

        return new ProductReviewResponse
        {
            Id = review.Id,
            ProductId = review.ProductId,
            UserName = $"{user.FirstName} {user.LastName}",
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }

    public async Task<ProductReviewResponse> UpdateAsync(
        string userId,
        int reviewId,
        CreateProductReviewRequest request)
    {
        var review = await _context.ProductReviews
            .FirstOrDefaultAsync(r =>
                r.Id == reviewId &&
                r.UserId == userId);

        if (review is null)
        {
            throw new NotFoundException(
                "Review not found.");
        }

        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.FirstName,
                u.LastName
            })
            .FirstOrDefaultAsync();

        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        review.Rating = request.Rating;
        review.Comment = request.Comment;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ProductReviewResponse
        {
            Id = review.Id,
            ProductId = review.ProductId,
            UserName = $"{user.FirstName} {user.LastName}",
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }

    public async Task DeleteAsync(
        string userId,
        int reviewId)
    {
        var review = await _context.ProductReviews
            .FirstOrDefaultAsync(r =>
                r.Id == reviewId &&
                r.UserId == userId);

        if (review is null)
        {
            throw new NotFoundException(
                "Review not found.");
        }

        _context.ProductReviews.Remove(review);

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ProductReviewResponse>> GetByProductIdAsync(
        int productId)
    {
        var productExists = await _context.Products
            .AnyAsync(p => p.Id == productId);

        if (!productExists)
        {
            throw new NotFoundException(
                "Product not found.");
        }

        return await _context.ProductReviews
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ProductReviewResponse
            {
                Id = r.Id,
                ProductId = r.ProductId,
                UserName = _context.Users
                    .Where(u => u.Id == r.UserId)
                    .Select(u => u.FirstName + " " + u.LastName)
                    .FirstOrDefault() ?? "Unknown User",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync();
    }

}