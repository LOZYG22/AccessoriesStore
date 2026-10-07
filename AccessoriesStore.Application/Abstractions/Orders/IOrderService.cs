using AccessoriesStore.Application.DTOs.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.Abstractions.Orders
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateAsync(
            string userId,
            CreateOrderRequest request);

        Task<OrderResponse> GetByIdAsync(
            string userId,
            int orderId);

        Task<List<OrderResponse>> GetAllAsync(
            string userId);

        Task<OrderResponse> ConfirmAsync(
            int orderId,
            ConfirmOrderRequest request);

        Task<List<OrderResponse>> GetAllForAdminAsync();

        Task<OrderResponse> UpdateStatusAsync(
            int orderId,
            UpdateOrderStatusRequest request);

        Task<OrderResponse> CancelAsync(string userId, int orderId);

        Task<List<OrderResponse>> GetHistoryAsync(string userId);

        Task CancelBySystemAsync(int orderId);
    }
}
