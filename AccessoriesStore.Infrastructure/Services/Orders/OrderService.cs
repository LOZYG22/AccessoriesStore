using AccessoriesStore.Application.Abstractions.Orders;
using AccessoriesStore.Application.DTOs.Addresses;
using AccessoriesStore.Application.DTOs.Orders;
using AccessoriesStore.Domain.Entities;
using AccessoriesStore.Domain.Enums;
using AccessoriesStore.Domain.Exceptions;
using AccessoriesStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessoriesStore.Infrastructure.Services.Orders
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrderResponse> CreateAsync(
            string userId,
            CreateOrderRequest request)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(x =>
                    x.Id == request.AddressId &&
                    x.UserId == userId);

            if (address is null)
                throw new NotFoundException("Address not found.");

            var cart = await _context.Carts
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (cart is null || !cart.Items.Any())
                throw new BadRequestException("Cart is empty.");

            foreach (var cartItem in cart.Items)
            {
                if (!cartItem.Product.IsActive)
                {
                    throw new BadRequestException(
                        $"Product '{cartItem.Product.Name}' is no longer available.");
                }

                if (cartItem.Quantity > cartItem.Product.StockQuantity)
                {
                    throw new BadRequestException(
                        $"Insufficient stock for product '{cartItem.Product.Name}'. " +
                        $"Only {cartItem.Product.StockQuantity} item(s) are available.");
                }
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var order = new Order
            {
                UserId = userId,
                AddressId = address.Id,

                ShippingFullName = address.FullName,
                ShippingPhoneNumber = address.PhoneNumber,
                ShippingCity = address.City,
                ShippingArea = address.Area,
                ShippingStreet = address.Street,
                ShippingBuildingNumber = address.BuildingNumber,
                ShippingApartmentNumber = address.ApartmentNumber,
                ShippingAdditionalDetails = address.AdditionalDetails,

                ShippingCost = 0,
                Discount = 0,
                Notes = request.Notes,
                Status = OrderStatus.Pending
            };

            foreach (var cartItem in cart.Items)
            {
                var unitPrice =
                    cartItem.Product.DiscountPrice ??
                    cartItem.Product.Price;

                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    ProductName = cartItem.Product.Name,
                    UnitPrice = unitPrice,
                    Quantity = cartItem.Quantity,
                    TotalPrice = unitPrice * cartItem.Quantity
                };

                order.Items.Add(orderItem);

                cartItem.Product.StockQuantity -= cartItem.Quantity;
            }

            order.SubTotal = order.Items.Sum(x => x.TotalPrice);

            order.Total =
                order.SubTotal +
                order.ShippingCost -
                order.Discount;

            _context.Orders.Add(order);

            _context.CartItems.RemoveRange(cart.Items);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return await GetByIdAsync(userId, order.Id);
        }

        public async Task<OrderResponse> GetByIdAsync(
            string userId,
            int orderId)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x =>
                    x.Id == orderId &&
                    x.UserId == userId);

            if (order is null)
                throw new NotFoundException("Order not found.");

            return MapToResponse(order);
        }

        public async Task<List<OrderResponse>> GetAllAsync(string userId)
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(x => x.Items)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return orders
                .Select(MapToResponse)
                .ToList();
        }

        public async Task<OrderResponse> ConfirmAsync(
            int orderId,
            ConfirmOrderRequest request)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order is null)
                throw new NotFoundException("Order not found.");

            if (order.Status != OrderStatus.Pending)
                throw new BadRequestException(
                    "Only pending orders can be confirmed.");

            if (request.ShippingCost < 0)
                throw new BadRequestException(
                    "Shipping cost cannot be negative.");

            if (request.Discount < 0)
                throw new BadRequestException(
                    "Discount cannot be negative.");

            var orderAmountBeforeDiscount =
                order.SubTotal + request.ShippingCost;

            if (request.Discount > orderAmountBeforeDiscount)
                throw new BadRequestException(
                    "Discount cannot be greater than the order amount.");

            order.ShippingCost = request.ShippingCost;
            order.Discount = request.Discount;

            order.Total =
                order.SubTotal +
                order.ShippingCost -
                order.Discount;

            order.Status = OrderStatus.Confirmed;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(order.UserId, order.Id);
        }

        public async Task<List<OrderResponse>> GetAllForAdminAsync()
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(x => x.Items)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return orders
                .Select(MapToResponse)
                .ToList();
        }

        public async Task<OrderResponse> UpdateStatusAsync(
            int orderId,
            UpdateOrderStatusRequest request)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order is null)
                throw new NotFoundException("Order not found.");

            if (string.IsNullOrWhiteSpace(request.Status))
                throw new BadRequestException(
                    "Order status is required.");

            if (!Enum.TryParse<OrderStatus>(
                request.Status,
                true,
                out var newStatus))
            {
                throw new BadRequestException(
                    "Invalid order status.");
            }

            if (!IsValidStatusTransition(order.Status, newStatus))
            {
                throw new BadRequestException(
                    $"Cannot change order status from {order.Status} to {newStatus}.");
            }

            order.Status = newStatus;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(order.UserId, order.Id);
        }

        public async Task<OrderResponse> CancelAsync(
            string userId,
            int orderId)
        {
            var order = await _context.Orders
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x =>
                    x.Id == orderId &&
                    x.UserId == userId);

            if (order is null)
                throw new NotFoundException("Order not found.");

            if (order.Status != OrderStatus.Pending &&
                order.Status != OrderStatus.Confirmed)
            {
                throw new BadRequestException(
                    "Only pending or confirmed orders can be cancelled.");
            }

            foreach (var orderItem in order.Items)
            {
                orderItem.Product.StockQuantity += orderItem.Quantity;
            }

            order.Status = OrderStatus.Cancelled;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(userId, order.Id);
        }

        public async Task<List<OrderResponse>> GetHistoryAsync(string userId)
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(x => x.Items)
                .Where(x =>
                    x.UserId == userId &&
                    (x.Status == OrderStatus.Delivered ||
                     x.Status == OrderStatus.Cancelled))
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return orders
                .Select(MapToResponse)
                .ToList();
        }

        public async Task CancelBySystemAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order is null)
                throw new NotFoundException("Order not found.");

            if (order.Status == OrderStatus.Cancelled)
                return;

            if (order.Status != OrderStatus.Pending &&
                order.Status != OrderStatus.Confirmed)
            {
                throw new BadRequestException(
                    "Only pending or confirmed orders can be cancelled.");
            }

            foreach (var orderItem in order.Items)
            {
                orderItem.Product.StockQuantity += orderItem.Quantity;
            }

            order.Status = OrderStatus.Cancelled;

            await _context.SaveChangesAsync();
        }

        private static bool IsValidStatusTransition(
            OrderStatus currentStatus,
            OrderStatus newStatus)
        {
            return currentStatus switch
            {
                OrderStatus.Pending =>
                    newStatus == OrderStatus.Confirmed ||
                    newStatus == OrderStatus.Cancelled,

                OrderStatus.Confirmed =>
                    newStatus == OrderStatus.Processing ||
                    newStatus == OrderStatus.Cancelled,

                OrderStatus.Processing =>
                    newStatus == OrderStatus.Shipped,

                OrderStatus.Shipped =>
                    newStatus == OrderStatus.Delivered,

                OrderStatus.Delivered => false,

                OrderStatus.Cancelled => false,

                _ => false
            };


        }

        private static OrderResponse MapToResponse(Order order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                SubTotal = order.SubTotal,
                ShippingCost = order.ShippingCost,
                Discount = order.Discount,
                Total = order.Total,
                Notes = order.Notes,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,

                Address = new AddressResponse
                {
                    Id = order.AddressId,
                    FullName = order.ShippingFullName,
                    PhoneNumber = order.ShippingPhoneNumber,
                    City = order.ShippingCity,
                    Area = order.ShippingArea,
                    Street = order.ShippingStreet,
                    BuildingNumber = order.ShippingBuildingNumber,
                    ApartmentNumber = order.ShippingApartmentNumber,
                    AdditionalDetails = order.ShippingAdditionalDetails
                },

                Items = order.Items
                    .Select(x => new OrderItemResponse
                    {
                        ProductId = x.ProductId,
                        ProductName = x.ProductName,
                        UnitPrice = x.UnitPrice,
                        Quantity = x.Quantity,
                        TotalPrice = x.TotalPrice
                    })
                    .ToList()
            };
        }
    }
}
