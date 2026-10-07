using AccessoriesStore.Application.Abstractions.Categories;
using AccessoriesStore.Application.DTOs.Categories;
using AccessoriesStore.Domain.Entities;
using AccessoriesStore.Domain.Exceptions;
using AccessoriesStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Infrastructure.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CategoryResponse> CreateAsync(
            CreateCategoryRequest request)
        {
            var slug = GenerateSlug(request.Name);

            var slugExists = await _context.Categories
                .AnyAsync(c => c.Slug == slug);

            if (slugExists)
            {
                throw new BadRequestException(
                    "A category with the same name already exists.");
            }

            var category = new Category
            {
                Name = request.Name,
                Slug = slug,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                DisplayOrder = request.DisplayOrder
            };

            _context.Categories.Add(category);

            await _context.SaveChangesAsync();

            return MapToResponse(category);
        }

        public async Task<CategoryResponse> GetByIdAsync(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category is null)
            {
                throw new NotFoundException("Category not found.");
            }

            return MapToResponse(category);
        }

        public async Task<List<CategoryResponse>> GetAllAsync()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return categories
                .Select(MapToResponse)
                .ToList();
        }

        public async Task<CategoryResponse> UpdateAsync(
            int id,
            UpdateCategoryRequest request)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category is null)
            {
                throw new NotFoundException("Category not found.");
            }

            var slug = GenerateSlug(request.Name);

            var slugExists = await _context.Categories
                .AnyAsync(c => c.Id != id && c.Slug == slug);

            if (slugExists)
            {
                throw new BadRequestException(
                    "A category with the same name already exists.");
            }

            category.Name = request.Name;
            category.Slug = slug;
            category.Description = request.Description;
            category.ImageUrl = request.ImageUrl;
            category.DisplayOrder = request.DisplayOrder;
            category.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return MapToResponse(category);
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category is null)
            {
                throw new NotFoundException("Category not found.");
            }

            var hasProducts = await _context.Products
                .AnyAsync(p => p.CategoryId == id);

            if (hasProducts)
            {
                throw new BadRequestException(
                    "Cannot delete a category that contains products. Move the products to another category first.");
            }

            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();
        }

        private static CategoryResponse MapToResponse(Category category)
        {
            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                IsActive = category.IsActive,
                DisplayOrder = category.DisplayOrder,
                CreatedAt = category.CreatedAt
            };
        }

        private static string GenerateSlug(string name)
        {
            return name
                .Trim()
                .ToLower()
                .Replace(" ", "-");
        }
    }
}
