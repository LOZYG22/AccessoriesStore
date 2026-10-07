using AccessoriesStore.Application.Abstractions.Images;
using AccessoriesStore.Application.Abstractions.Products;
using AccessoriesStore.Application.DTOs.Products;
using AccessoriesStore.Domain.Entities;
using AccessoriesStore.Domain.Exceptions;
using AccessoriesStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessoriesStore.Infrastructure.Services.Products
{
    public class ProductImageService : IProductImageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageStorageService _imageStorage;

        public ProductImageService(
            ApplicationDbContext context,
            IImageStorageService imageStorage)
        {
            _context = context;
            _imageStorage = imageStorage;
        }

        public async Task<ProductImageResponse> UploadAsync(
            int productId,
            UploadProductImageRequest request)
        {
            var productExists = await _context.Products
                .AnyAsync(p => p.Id == productId);

            if (!productExists)
            {
                throw new NotFoundException("Product not found.");
            }

            var uploadResult = await _imageStorage.UploadAsync(
                request.File,
                request.FileName,
                request.ContentType);

            var displayOrder = await _context.ProductImages
                .Where(x => x.ProductId == productId)
                .Select(x => (int?)x.DisplayOrder)
                .MaxAsync() ?? -1;

            displayOrder++;

            var hasImages = await _context.ProductImages
                .AnyAsync(x => x.ProductId == productId);

            var image = new ProductImage
            {
                ProductId = productId,
                ImageUrl = uploadResult.ImageUrl,
                PublicId = uploadResult.PublicId,
                IsMain = !hasImages,
                DisplayOrder = displayOrder
            };

            _context.ProductImages.Add(image);

            await _context.SaveChangesAsync();

            return new ProductImageResponse
            {
                Id = image.Id,
                ImageUrl = image.ImageUrl,
                IsMain = image.IsMain,
                DisplayOrder = image.DisplayOrder
            };
        }

        public async Task<List<ProductImageResponse>> GetByProductIdAsync(
            int productId)
        {
            var productExists = await _context.Products
                .AnyAsync(p => p.Id == productId);

            if (!productExists)
            {
                throw new NotFoundException("Product not found.");
            }

            var images = await _context.ProductImages
                .AsNoTracking()
                .Where(x => x.ProductId == productId)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            return images
                .Select(x => new ProductImageResponse
                {
                    Id = x.Id,
                    ImageUrl = x.ImageUrl,
                    IsMain = x.IsMain,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();
        }

        public async Task SetMainImageAsync(
            int productId,
            int imageId)
        {
            var image = await _context.ProductImages
                .FirstOrDefaultAsync(x =>
                    x.Id == imageId &&
                    x.ProductId == productId);

            if (image is null)
            {
                throw new NotFoundException("Image not found.");
            }

            var productImages = await _context.ProductImages
                .Where(x => x.ProductId == productId)
                .ToListAsync();

            foreach (var productImage in productImages)
            {
                productImage.IsMain = false;
            }

            image.IsMain = true;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(
            int productId,
            int imageId)
        {
            var image = await _context.ProductImages
                .FirstOrDefaultAsync(x =>
                    x.Id == imageId &&
                    x.ProductId == productId);

            if (image is null)
            {
                throw new NotFoundException("Image not found.");
            }

            var wasMain = image.IsMain;
            var publicId = image.PublicId;

            _context.ProductImages.Remove(image);

            if (wasMain)
            {
                var nextMainImage = await _context.ProductImages
                    .Where(x =>
                        x.ProductId == productId &&
                        x.Id != imageId)
                    .OrderBy(x => x.DisplayOrder)
                    .FirstOrDefaultAsync();

                if (nextMainImage is not null)
                {
                    nextMainImage.IsMain = true;
                }
            }

            await _context.SaveChangesAsync();

            await _imageStorage.DeleteAsync(publicId);
        }
    }
}