using AccessoriesStore.Application.Abstractions.Inventory;
using AccessoriesStore.Application.Common.Responses;
using AccessoriesStore.Application.DTOs.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessoriesStore.Api.Controllers;

[ApiController]
[Route("api/admin/inventory")]
[Authorize(Roles = "Admin")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpPost("products/{productId}/add")]
    public async Task<ActionResult<ApiResponse<object>>> AddStock(
        int productId,
        UpdateStockRequest request)
    {
        await _inventoryService.AddStockAsync(
            productId,
            request);

        return Ok(
            ApiResponse<object>.SuccessResponse(
                null,
                "Stock added successfully."));
    }

    [HttpPost("products/{productId}/remove")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveStock(
        int productId,
        UpdateStockRequest request)
    {
        await _inventoryService.RemoveStockAsync(
            productId,
            request);

        return Ok(
            ApiResponse<object>.SuccessResponse(
                null,
                "Stock removed successfully."));
    }
}