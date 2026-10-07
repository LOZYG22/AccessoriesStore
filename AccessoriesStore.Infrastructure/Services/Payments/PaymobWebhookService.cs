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

        var order = await _context.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x =>
                x.PaymobOrderId == callback.Order.Id);

        if (order is null)
        {
            throw new NotFoundException(
                "Order linked to Paymob transaction was not found.");
        }

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
            order.PaymentStatus = PaymentStatus.Refunded;
        }
        else if (callback.IsVoided)
        {
            order.PaymentStatus = PaymentStatus.Cancelled;
        }
        else if (callback.Success && !callback.Pending)
        {
            if (order.PaymentStatus != PaymentStatus.Paid)
            {
                order.PaymentStatus = PaymentStatus.Paid;
                order.PaidAt = DateTime.UtcNow;
            }
        }
        else if (!callback.Success && !callback.Pending)
        {
            order.PaymentStatus = PaymentStatus.Failed;

            await _orderService.CancelBySystemAsync(order.Id);
        }

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