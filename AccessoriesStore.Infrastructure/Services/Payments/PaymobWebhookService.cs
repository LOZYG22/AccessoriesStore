using AccessoriesStore.Application.Abstractions.Orders;
using AccessoriesStore.Application.DTOs.Payments;
using AccessoriesStore.Domain.Enums;
using AccessoriesStore.Domain.Exceptions;
using AccessoriesStore.Infrastructure.Persistence;
using AccessoriesStore.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace AccessoriesStore.Infrastructure.Services.Payments;

public class PaymobWebhookService
{
    private readonly ApplicationDbContext _context;
    private readonly PaymobSettings _settings;
    private readonly IOrderService _orderService;

    public PaymobWebhookService(
        ApplicationDbContext context,
        IOptions<PaymobSettings> options,
        IOrderService orderService)
    {
        _context = context;
        _settings = options.Value;
        _orderService = orderService;
    }

    public async Task ProcessTransactionAsync(
        PaymobTransactionCallbackRequest request,
        string receivedHmac)
    {
        if (string.IsNullOrWhiteSpace(receivedHmac))
        {
            throw new UnauthorizedException(
                "Missing Paymob HMAC.");
        }

        if (string.IsNullOrWhiteSpace(_settings.HmacSecret))
        {
            throw new InvalidOperationException(
                "Paymob HMAC secret is not configured.");
        }

        if (!VerifyHmac(request.Obj, receivedHmac))
        {
            throw new UnauthorizedException(
                "Invalid Paymob HMAC.");
        }

        if (!string.Equals(
                request.Type,
                "TRANSACTION",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException(
                "Unsupported Paymob callback type.");
        }

        var callback = request.Obj;

        if (callback.Order.Id <= 0)
        {
            throw new BadRequestException(
                "Invalid Paymob order ID.");
        }

        if (!_settings.PaymentMethodIds.Contains((int)callback.IntegrationId))
            throw new UnauthorizedException(
                "Paymob integration is not configured for this store.");

        var order = await _context.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x =>
                x.PaymobOrderId == callback.Order.Id);

        if (order is null)
        {
            throw new NotFoundException(
                "Order linked to Paymob transaction was not found.");
        }

        var paymentAttempt = await _context.PaymentAttempts
            .FirstOrDefaultAsync(x => x.PaymobOrderId == callback.Order.Id);

        if (!string.Equals(
                callback.Currency,
                "EGP",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException(
                "Invalid payment currency.");
        }

        var expectedAmountCents = (long)Math.Round(
            order.Total * 100,
            MidpointRounding.AwayFromZero);

        if (callback.AmountCents != expectedAmountCents)
        {
            throw new BadRequestException(
                "Payment amount does not match the order total.");
        }

        if (callback.IsRefunded)
        {
            var refundedAmount = callback.RefundedAmountCents ?? 0;
            if (paymentAttempt is not null)
            {
                var refundIncreased = refundedAmount > paymentAttempt.RefundedAmountCents;
                paymentAttempt.RefundedAmountCents = Math.Max(paymentAttempt.RefundedAmountCents, refundedAmount);
                paymentAttempt.Status = refundedAmount >= expectedAmountCents ? "Refunded" : "PartiallyRefunded";
                paymentAttempt.UpdatedAt = DateTime.UtcNow;
                paymentAttempt.PaymobTransactionId = callback.Id;
                await _context.SaveChangesAsync();
                if (refundIncreased && refundedAmount < expectedAmountCents)
                    await NotifyPaymentAsync(order.UserId, "Partial refund processed", $"A partial refund for order #{order.Id} has been recorded.", $"/account/orders/{order.Id}");
            }
            // Paymob can send callbacks for partial refunds. Keep the order
            // paid until the cumulative refunded amount covers the charge.
            if (refundedAmount >= expectedAmountCents)
            {
                var refunded = await _context.Orders
                    .Where(x => x.Id == order.Id &&
                                x.PaymentStatus == PaymentStatus.Paid)
                    .ExecuteUpdateAsync(updates => updates
                        .SetProperty(x => x.PaymentStatus, PaymentStatus.Refunded));
                if (refunded == 1)
                    await NotifyPaymentAsync(order.UserId, "Refund completed", $"Your refund for order #{order.Id} has been confirmed.", $"/account/orders/{order.Id}");
            }
        }
        else if (callback.IsVoided)
        {
            var changed = await _context.Orders
                .Where(x => x.Id == order.Id &&
                            x.PaymentStatus == PaymentStatus.Pending)
                .ExecuteUpdateAsync(updates => updates
                    .SetProperty(x => x.PaymentStatus, PaymentStatus.Cancelled));

            if (paymentAttempt is not null)
            {
                paymentAttempt.Status = "Voided";
                paymentAttempt.PaymobTransactionId = callback.Id;
                paymentAttempt.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            if (changed == 1 &&
                (order.Status == OrderStatus.Pending ||
                 order.Status == OrderStatus.Confirmed))
            {
                await _orderService.CancelBySystemAsync(order.Id);
            }
        }
        else if (callback.Success && !callback.Pending)
        {
            var paid = await _context.Orders
                .Where(x => x.Id == order.Id &&
                            (x.PaymentStatus == PaymentStatus.Pending ||
                             x.PaymentStatus == PaymentStatus.Failed ||
                             x.PaymentStatus == PaymentStatus.Cancelled))
                .ExecuteUpdateAsync(updates => updates
                    .SetProperty(x => x.PaymentStatus, PaymentStatus.Paid)
                    .SetProperty(x => x.PaidAt, DateTime.UtcNow));
            if (paid == 1)
                await NotifyPaymentAsync(order.UserId, "Payment received", $"Payment for order #{order.Id} was successful.", $"/account/orders/{order.Id}");
            if (paid == 1 && paymentAttempt is not null)
            {
                paymentAttempt.Status = "Paid";
                paymentAttempt.PaymobTransactionId = callback.Id;
                paymentAttempt.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        else if (!callback.Success && !callback.Pending)
        {
            var changed = await _context.Orders
                .Where(x => x.Id == order.Id &&
                            x.PaymentStatus == PaymentStatus.Pending)
                .ExecuteUpdateAsync(updates => updates
                    .SetProperty(x => x.PaymentStatus, PaymentStatus.Failed)
                    .SetProperty(x => x.PaymentIntentId, (string?)null)
                    .SetProperty(x => x.PaymobOrderId, (long?)null));
            if (paymentAttempt is not null)
            {
                paymentAttempt.Status = "Failed";
                paymentAttempt.PaymobTransactionId = callback.Id;
                paymentAttempt.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            if (changed == 1)
                await NotifyPaymentAsync(order.UserId, "Payment unsuccessful", $"Payment for order #{order.Id} was unsuccessful. You can retry payment from your orders.", $"/account/orders/{order.Id}");
        }
    }

    private async Task NotifyPaymentAsync(string userId, string title, string message, string link)
    {
        _context.Notifications.Add(new AccessoriesStore.Domain.Entities.StoreNotification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = "Payment",
            Link = link
        });
        await _context.SaveChangesAsync();
    }

    private bool VerifyHmac(
        PaymobTransactionObject transaction,
        string receivedHmac)
    {
        var data =
            $"{transaction.AmountCents}" +
            $"{transaction.CreatedAt}" +
            $"{transaction.Currency}" +
            $"{transaction.ErrorOccured.ToString().ToLowerInvariant()}" +
            $"{transaction.HasParentTransaction.ToString().ToLowerInvariant()}" +
            $"{transaction.Id}" +
            $"{transaction.IntegrationId}" +
            $"{transaction.Is3DSecure.ToString().ToLowerInvariant()}" +
            $"{transaction.IsAuth.ToString().ToLowerInvariant()}" +
            $"{transaction.IsCapture.ToString().ToLowerInvariant()}" +
            $"{transaction.IsRefunded.ToString().ToLowerInvariant()}" +
            $"{transaction.IsStandalonePayment.ToString().ToLowerInvariant()}" +
            $"{transaction.IsVoided.ToString().ToLowerInvariant()}" +
            $"{transaction.Order.Id}" +
            $"{transaction.Owner}" +
            $"{transaction.Pending.ToString().ToLowerInvariant()}" +
            $"{transaction.SourceData.Pan}" +
            $"{transaction.SourceData.SubType}" +
            $"{transaction.SourceData.Type}" +
            $"{transaction.Success.ToString().ToLowerInvariant()}";

        using var hmac = new HMACSHA512(
            Encoding.UTF8.GetBytes(_settings.HmacSecret));

        var computedHash = hmac.ComputeHash(
            Encoding.UTF8.GetBytes(data));

        var computedHmac = Convert.ToHexString(
            computedHash).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHmac),
            Encoding.UTF8.GetBytes(receivedHmac));
    }
}
