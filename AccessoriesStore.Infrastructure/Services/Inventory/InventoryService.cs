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

        var updatedAt = DateTime.UtcNow;
        var changed = await _context.Products
            .Where(p => p.Id == productId)
            .ExecuteUpdateAsync(updates => updates
                .SetProperty(
                    p => p.StockQuantity,
                    p => p.StockQuantity + request.Quantity)
                .SetProperty(p => p.UpdatedAt, updatedAt));

        if (changed == 0)
            throw new NotFoundException("Product not found.");
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

        var updatedAt = DateTime.UtcNow;
        var changed = await _context.Products
            .Where(p => p.Id == productId && p.StockQuantity >= request.Quantity)
            .ExecuteUpdateAsync(updates => updates
                .SetProperty(
                    p => p.StockQuantity,
                    p => p.StockQuantity - request.Quantity)
                .SetProperty(p => p.UpdatedAt, updatedAt));

        if (changed == 0)
        {
            var exists = await _context.Products
                .AnyAsync(p => p.Id == productId);

            if (!exists)
                throw new NotFoundException("Product not found.");

            throw new BadRequestException("Insufficient stock.");
        }
    }
}
