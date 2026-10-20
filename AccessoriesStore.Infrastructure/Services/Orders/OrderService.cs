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

                var stockChanged = await _context.Products
                    .Where(product => product.Id == cartItem.ProductId &&
                                      product.IsActive &&
                                      product.StockQuantity >= cartItem.Quantity)
                    .ExecuteUpdateAsync(updates => updates
                        .SetProperty(
                            product => product.StockQuantity,
                            product => product.StockQuantity - cartItem.Quantity)
                        .SetProperty(
                            product => product.UpdatedAt,
                            DateTime.UtcNow));

                if (stockChanged != 1)
                    throw new BadRequestException(
                        $"Insufficient stock for product '{cartItem.Product.Name}'. Please refresh your cart.");
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

            await NotifyAsync(userId, "Order received", $"Your order #{order.Id} has been received.", $"/account/orders/{order.Id}");

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
            await NotifyAsync(order.UserId, "Order confirmed", $"Your order #{order.Id} is ready for payment.", $"/account/orders/{order.Id}");

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

            if (newStatus == OrderStatus.Cancelled &&
                (order.PaymentStatus == PaymentStatus.Paid ||
                 (order.PaymentStatus == PaymentStatus.Pending &&
                  !string.IsNullOrWhiteSpace(order.PaymentIntentId))))
                throw new BadRequestException(
                    "Orders with a completed payment or an active payment session cannot be cancelled here.");

            if (newStatus == OrderStatus.Cancelled)
            {
                await CancelBySystemAsync(orderId);
                return await GetByIdAsync(order.UserId, order.Id);
            }

            if ((newStatus == OrderStatus.Processing ||
                 newStatus == OrderStatus.Shipped ||
                 newStatus == OrderStatus.Delivered) &&
                order.PaymentStatus != PaymentStatus.Paid)
                throw new BadRequestException(
                    "Only paid orders can move into fulfillment.");

            var changed = await _context.Orders
                .Where(x => x.Id == orderId && x.Status == order.Status)
                .ExecuteUpdateAsync(updates => updates
                    .SetProperty(x => x.Status, newStatus));

            if (changed != 1)
                throw new BadRequestException(
                    "The order changed while this update was being processed. Refresh and try again.");

            await NotifyAsync(order.UserId, "Order updated", $"Your order #{order.Id} is now {newStatus}.", $"/account/orders/{order.Id}");

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

            if (order.PaymentStatus == PaymentStatus.Paid)
                throw new BadRequestException(
                    "Paid orders cannot be cancelled. Contact support to arrange a refund.");

            if (order.PaymentStatus == PaymentStatus.Pending &&
                !string.IsNullOrWhiteSpace(order.PaymentIntentId))
                throw new BadRequestException(
                    "An active payment session exists for this order. Complete or close it before cancelling.");

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var changed = await _context.Orders
                .Where(x => x.Id == orderId &&
                            x.UserId == userId &&
                            (x.Status == OrderStatus.Pending ||
                             x.Status == OrderStatus.Confirmed) &&
                            x.PaymentStatus != PaymentStatus.Paid &&
                            !(x.PaymentStatus == PaymentStatus.Pending &&
                              x.PaymentIntentId != null))
                .ExecuteUpdateAsync(updates => updates
                    .SetProperty(x => x.Status, OrderStatus.Cancelled));

            if (changed != 1)
                throw new BadRequestException(
                    "The order can no longer be cancelled. Refresh and try again.");

            foreach (var orderItem in order.Items)
            {
                await _context.Products
                    .Where(product => product.Id == orderItem.ProductId)
                    .ExecuteUpdateAsync(updates => updates
                        .SetProperty(
                            product => product.StockQuantity,
                            product => product.StockQuantity + orderItem.Quantity)
                        .SetProperty(
                            product => product.UpdatedAt,
                            DateTime.UtcNow));
            }

            await transaction.CommitAsync();

            await NotifyAsync(userId, "Order cancelled", $"Your order #{order.Id} has been cancelled.", $"/account/orders/{order.Id}");

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

            if (order.PaymentStatus == PaymentStatus.Paid)
                throw new BadRequestException(
                    "Paid orders cannot be automatically cancelled.");

            if (order.PaymentStatus == PaymentStatus.Pending &&
                !string.IsNullOrWhiteSpace(order.PaymentIntentId))
                throw new BadRequestException(
                    "An active payment session exists for this order; cancel the payment session before cancelling the order.");

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var changed = await _context.Orders
                .Where(x => x.Id == orderId &&
                            (x.Status == OrderStatus.Pending ||
                             x.Status == OrderStatus.Confirmed) &&
                            x.PaymentStatus != PaymentStatus.Paid)
                .ExecuteUpdateAsync(updates => updates
                    .SetProperty(x => x.Status, OrderStatus.Cancelled));

            if (changed == 0)
            {
                await transaction.CommitAsync();
                return;
            }

            foreach (var orderItem in order.Items)
            {
                await _context.Products
                    .Where(product => product.Id == orderItem.ProductId)
                    .ExecuteUpdateAsync(updates => updates
                        .SetProperty(
                            product => product.StockQuantity,
                            product => product.StockQuantity + orderItem.Quantity)
                        .SetProperty(
                            product => product.UpdatedAt,
                            DateTime.UtcNow));
            }

            await transaction.CommitAsync();

            await NotifyAsync(order.UserId, "Order cancelled", $"Your order #{order.Id} has been cancelled.", $"/account/orders/{order.Id}");
        }

        private async Task NotifyAsync(string userId, string title, string message, string link)
        {
            _context.Notifications.Add(new StoreNotification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = "Order",
                Link = link
            });
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
                PaymentStatus = order.PaymentStatus.ToString(),
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
