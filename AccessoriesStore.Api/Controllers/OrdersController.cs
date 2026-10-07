using AccessoriesStore.Application.Abstractions.Orders;
using AccessoriesStore.Application.Common.Responses;
using AccessoriesStore.Application.DTOs.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AccessoriesStore.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrdersController(
           IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _orderService.CreateAsync(
                userId!,
                request);

            return Ok(
                    ApiResponse<OrderResponse>.SuccessResponse(
                        result,
                        "Order created successfully.")
                );
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _orderService.GetAllAsync(userId!);

            return Ok(
                    ApiResponse<List<OrderResponse>>.SuccessResponse(result)
                );
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _orderService.GetByIdAsync(
                userId!,
                id);

            return Ok(
                    ApiResponse<OrderResponse>.SuccessResponse(result)
                );
        }

        [HttpPut("{id:int}/confirm")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Confirm(
            int id,
            ConfirmOrderRequest request)
        {
            var result = await _orderService.ConfirmAsync(
                id,
                request);

            return Ok(
                    ApiResponse<OrderResponse>.SuccessResponse(
                        result,
                        "Order confirmed successfully.")
                );
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllForAdmin()
        {
            var result = await _orderService.GetAllForAdminAsync();

            return Ok(
                    ApiResponse<List<OrderResponse>>.SuccessResponse(result)
                );
        }

        [HttpPut("admin/{id:int}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            UpdateOrderStatusRequest request)
        {
            var result = await _orderService.UpdateStatusAsync(id, request);

            return Ok(
                    ApiResponse<OrderResponse>.SuccessResponse(
                        result,
                        "Order status updated successfully.")
                );
        }

        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _orderService.CancelAsync(userId!, id);

            return Ok(
                    ApiResponse<OrderResponse>.SuccessResponse(
                        result,
                        "Order cancelled successfully.")
                );
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _orderService.GetHistoryAsync(userId!);

            return Ok(
                    ApiResponse<List<OrderResponse>>.SuccessResponse(result)
                );
        }
    }
}
