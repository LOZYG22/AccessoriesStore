using AccessoriesStore.Application.Abstractions.Common;
using AccessoriesStore.Application.Abstractions.Products;
using AccessoriesStore.Application.DTOs.Products;
using AccessoriesStore.Domain.Entities;
using AccessoriesStore.Domain.Exceptions;
using AccessoriesStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessoriesStore.Infrastructure.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISlugGenerator _slugGenerator;

        public ProductService(
            ApplicationDbContext context,
            ISlugGenerator slugGenerator)
        {
            _context = context;
            _slugGenerator = slugGenerator;
        }

        private async Task<string> GenerateUniqueSlugAsync(string name)
        {
            var baseSlug = _slugGenerator.Generate(name);

            var slug = baseSlug;
            var counter = 2;

            while (await _context.Products.AnyAsync(p => p.Slug == slug))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        private static ProductRatingResponse CalculateRating(
            ICollection<ProductReview> reviews)
        {
            if (reviews.Count == 0)
            {
                return new ProductRatingResponse();
            }

            return new ProductRatingResponse
            {
                Average = Math.Round(
                    reviews.Average(r => r.Rating),
                    1),

                Count = reviews.Count,

                FiveStars = reviews.Count(r => r.Rating == 5),
                FourStars = reviews.Count(r => r.Rating == 4),
                ThreeStars = reviews.Count(r => r.Rating == 3),
                TwoStars = reviews.Count(r => r.Rating == 2),
                OneStar = reviews.Count(r => r.Rating == 1)
            };
        }

        public async Task<ProductResponse> CreateAsync(
            CreateProductRequest request)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == request.CategoryId);

            if (!categoryExists)
            {
                throw new NotFoundException("Category not found.");
            }

            var slug = await GenerateUniqueSlugAsync(request.Name);

            var skuExists = await _context.Products
                .AnyAsync(p => p.SKU == request.SKU);

            if (skuExists)
            {
                throw new BadRequestException(
                    "A product with the same SKU already exists.");
            }

            var product = new Product
            {
                Name = request.Name,
                Slug = slug,
                Description = request.Description,
                Price = request.Price,
                DiscountPrice = request.DiscountPrice,
                SKU = request.SKU,
                StockQuantity = request.StockQuantity,
                CategoryId = request.CategoryId,
                IsFeatured = request.IsFeatured
            };

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                SKU = product.SKU,
                CategoryId = product.CategoryId,
                IsFeatured = product.IsFeatured,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        public async Task<ProductResponse> GetByIdAsync(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                throw new NotFoundException("Product not found.");
            }

            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                SKU = product.SKU,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                IsActive = product.IsActive,
                IsFeatured = product.IsFeatured,
                Rating = CalculateRating(product.Reviews),
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Images = product.Images
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => new ProductImageResponse
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        IsMain = i.IsMain,
                        DisplayOrder = i.DisplayOrder
                    })
                    .ToList()
            };
        }

        public async Task<ProductResponse> GetBySlugAsync(string slug)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Slug == slug);

            if (product is null)
            {
                throw new NotFoundException("Product not found.");
            }

            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                SKU = product.SKU,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                IsActive = product.IsActive,
                IsFeatured = product.IsFeatured,
                Rating = CalculateRating(product.Reviews),
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Images = product.Images
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => new ProductImageResponse
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        IsMain = i.IsMain,
                        DisplayOrder = i.DisplayOrder
                    })
                    .ToList()
            };
        }

        public async Task<ProductPagedResponse> GetAllAsync(
            ProductFilterRequest request)
        {
            var query = _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();

                query = query.Where(p =>
                    p.Name.Contains(search));
            }

            // Category
            if (request.CategoryId.HasValue)
            {
                query = query.Where(p =>
                    p.CategoryId == request.CategoryId.Value);
            }

            // Price
            if (request.MinPrice.HasValue)
            {
                query = query.Where(p =>
                    (p.DiscountPrice ?? p.Price)
                    >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(p =>
                    (p.DiscountPrice ?? p.Price)
                    <= request.MaxPrice.Value);
            }

            // In Stock
            if (request.InStock.HasValue)
            {
                if (request.InStock.Value)
                {
                    query = query.Where(p =>
                        p.StockQuantity > 0);
                }
                else
                {
                    query = query.Where(p =>
                        p.StockQuantity == 0);
                }
            }

            // Featured
            if (request.IsFeatured.HasValue)
            {
                query = query.Where(p =>
                    p.IsFeatured == request.IsFeatured.Value);
            }

            // Total Count
            var totalCount = await query.CountAsync();

            // Sorting
            query = request.SortBy?.ToLower() switch
            {
                "priceasc" => query.OrderBy(p =>
                    p.DiscountPrice ?? p.Price),

                "pricedesc" => query.OrderByDescending(p =>
                    p.DiscountPrice ?? p.Price),

                "rating" => query.OrderByDescending(p =>
                    p.Reviews
                        .Average(r => (double?)r.Rating) ?? 0),

                _ => query.OrderByDescending(p =>
                    p.CreatedAt)
            };

            // Pagination
            var page = request.Page < 1
                ? 1
                : request.Page;

            var pageSize = request.PageSize < 1
                ? 12
                : Math.Min(request.PageSize, 50);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(product => new ProductResponse
                {
                    Id = product.Id,
                    Name = product.Name,
                    Slug = product.Slug,
                    Description = product.Description,
                    Price = product.Price,
                    DiscountPrice = product.DiscountPrice,
                    SKU = product.SKU,
                    StockQuantity = product.StockQuantity,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category.Name,
                    IsActive = product.IsActive,
                    IsFeatured = product.IsFeatured,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt,

                    Images = product.Images
                        .Where(i => i.IsMain)
                        .OrderBy(i => i.DisplayOrder)
                        .Select(i => new ProductImageResponse
                        {
                            Id = i.Id,
                            ImageUrl = i.ImageUrl,
                            IsMain = i.IsMain,
                            DisplayOrder = i.DisplayOrder
                        })
                        .ToList(),

                    Rating = new ProductRatingResponse
                    {
                        Average = product.Reviews
                            .Select(r => (double?)r.Rating)
                            .Average() ?? 0,

                        Count = product.Reviews.Count(),

                        FiveStars = product.Reviews
                            .Count(r => r.Rating == 5),

                        FourStars = product.Reviews
                            .Count(r => r.Rating == 4),

                        ThreeStars = product.Reviews
                            .Count(r => r.Rating == 3),

                        TwoStars = product.Reviews
                            .Count(r => r.Rating == 2),

                        OneStar = product.Reviews
                            .Count(r => r.Rating == 1)
                    }
                })
                .ToListAsync();

            return new ProductPagedResponse
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(
                    totalCount / (double)pageSize)
            };
        }
        public async Task<List<ProductResponse>> GetAllForAdminAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Select(product => new ProductResponse
                {
                    Id = product.Id,
                    Name = product.Name,
                    Slug = product.Slug,
                    Description = product.Description,
                    Price = product.Price,
                    DiscountPrice = product.DiscountPrice,
                    SKU = product.SKU,
                    StockQuantity = product.StockQuantity,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category.Name,
                    IsActive = product.IsActive,
                    IsFeatured = product.IsFeatured,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt,

                    Images = product.Images
                        .OrderBy(i => i.DisplayOrder)
                        .Select(i => new ProductImageResponse
                        {
                            Id = i.Id,
                            ImageUrl = i.ImageUrl,
                            IsMain = i.IsMain,
                            DisplayOrder = i.DisplayOrder
                        })
                        .ToList(),

                    Rating = new ProductRatingResponse
                    {
                        Average = product.Reviews
                            .Select(r => (double?)r.Rating)
                            .Average() ?? 0,

                        Count = product.Reviews.Count(),

                        FiveStars = product.Reviews
                            .Count(r => r.Rating == 5),

                        FourStars = product.Reviews
                            .Count(r => r.Rating == 4),

                        ThreeStars = product.Reviews
                            .Count(r => r.Rating == 3),

                        TwoStars = product.Reviews
                            .Count(r => r.Rating == 2),

                        OneStar = product.Reviews
                            .Count(r => r.Rating == 1)
                    }
                })
                .ToListAsync();
        }

        public async Task<ProductResponse> UpdateAsync(
            int id,
            UpdateProductRequest request)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                throw new NotFoundException("Product not found.");
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId);

            if (category is null)
                throw new NotFoundException("Category not found.");


            var skuExists = await _context.Products
                .AnyAsync(p => p.Id != id && p.SKU == request.SKU);

            if (skuExists)
            {
                throw new BadRequestException(
                    "A product with the same SKU already exists.");
            }

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.DiscountPrice = request.DiscountPrice;
            product.SKU = request.SKU;
            product.CategoryId = category.Id;
            product.Category = category;
            product.IsActive = request.IsActive;
            product.IsFeatured = request.IsFeatured;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                SKU = product.SKU,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                IsActive = product.IsActive,
                IsFeatured = product.IsFeatured,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,

                Images = product.Images
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => new ProductImageResponse
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        IsMain = i.IsMain,
                        DisplayOrder = i.DisplayOrder
                    })
                    .ToList()
            };
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                throw new NotFoundException("Product not found.");
            }

            if (!product.IsActive)
            {
                throw new BadRequestException(
                    "Product is already inactive.");
            }

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
