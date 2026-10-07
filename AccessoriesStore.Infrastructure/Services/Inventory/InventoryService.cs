using AccessoriesStore.Application.Abstractions.Inventory;
using AccessoriesStore.Application.DTOs.Inventory;
using AccessoriesStore.Domain.Exceptions;
using AccessoriesStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessoriesStore.Infrastructure.Services.Inventory;

public class InventoryService : IInventoryService
{
    private readonly ApplicationDbContext _context;

    public InventoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddStockAsync(
        int productId,
        UpdateStockRequest request)
    {
        if (request.Quantity <= 0)
        {
            throw new BadRequestException(
                "Quantity must be greater than zero.");
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product is null)
        {
            throw new NotFoundException(
                "Product not found.");
        }

        product.StockQuantity += request.Quantity;

        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task RemoveStockAsync(
        int productId,
        UpdateStockRequest request)
    {
        if (request.Quantity <= 0)
        {
            throw new BadRequestException(
                "Quantity must be greater than zero.");
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product is null)
        {
            throw new NotFoundException(
                "Product not found.");
        }

        if (product.StockQuantity < request.Quantity)
        {
            throw new BadRequestException(
                "Insufficient stock.");
        }

        product.StockQuantity -= request.Quantity;

        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}