using AccessoriesStore.Application.DTOs.Inventory;

namespace AccessoriesStore.Application.Abstractions.Inventory
{
    public interface IInventoryService
    {
        Task AddStockAsync(
            int productId,
            UpdateStockRequest request);

        Task RemoveStockAsync(
            int productId,
            UpdateStockRequest request);
    }
}
