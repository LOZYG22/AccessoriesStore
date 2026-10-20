using AccessoriesStore.Application.Abstractions.Payments;
using AccessoriesStore.Application.DTOs.Payments;
using AccessoriesStore.Domain.Enums;
using AccessoriesStore.Domain.Entities;
using AccessoriesStore.Domain.Exceptions;
using AccessoriesStore.Infrastructure.Identity;
using AccessoriesStore.Infrastructure.Persistence;
using AccessoriesStore.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AccessoriesStore.Infrastructure.Services.Payments
{
    public class PaymobPaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly PaymobSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymobPaymentService(
            ApplicationDbContext context,
            IOptions<PaymobSettings> options,
            HttpClient httpClient,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _settings = options.Value;
            _httpClient = httpClient;
            _userManager = userManager;
        }

        public async Task<PaymentResponse> CreatePaymentAsync(
            string userId,
            CreatePaymentRequest request)
        {
            var order = await _context.Orders
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x =>
                    x.Id == request.OrderId &&
                    x.UserId == userId);

            if (order is null)
                throw new NotFoundException("Order not found.");

            if (order.PaymentStatus == PaymentStatus.Paid)
                throw new BadRequestException(
                    "Order has already been paid.");

            if (order.Status == OrderStatus.Cancelled)
                throw new BadRequestException(
                    "Cancelled orders cannot be paid.");

            if (order.PaymentStatus != PaymentStatus.Pending &&
                order.PaymentStatus != PaymentStatus.Failed)
                throw new BadRequestException(
                    "Order is not available for payment.");

            if (order.Status != OrderStatus.Confirmed)
                throw new BadRequestException(
                    "Only confirmed orders can be paid.");

            if (order.PaymentStatus != PaymentStatus.Failed &&
                (!string.IsNullOrWhiteSpace(order.PaymentIntentId) || order.PaymobOrderId.HasValue))
                throw new BadRequestException(
                    "A payment session has already been created for this order.");

            if (order.Total <= 0)
                throw new BadRequestException(
                    "Order total must be greater than zero.");

            if (order.Items.Count == 0)
                throw new BadRequestException(
                    "Order has no items.");

            if (string.IsNullOrWhiteSpace(_settings.SecretKey))
                throw new InvalidOperationException(
                    "Paymob secret key is not configured.");

            if (_settings.PaymentMethodIds.Count == 0)
                throw new InvalidOperationException(
                    "Paymob payment methods are not configured.");

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                throw new NotFoundException("User not found.");

            var amountInCents = (long)Math.Round(
                order.Total * 100,
                MidpointRounding.AwayFromZero);

            var nameParts = order.ShippingFullName
                .Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

            var firstName = nameParts.Length > 0
                ? nameParts[0]
                : "Customer";

            var lastName = nameParts.Length > 1
                ? string.Join(" ", nameParts.Skip(1))
                : "Customer";

            // Paymob requires item amounts (amount × quantity) to sum to
            // the intention amount. One summary item keeps shipping and
            // discounts in sync with the server-calculated order total.
            var items = new[]
            {
                new
                {
                    name = $"Accessories Store order #{order.Id}",
                    amount = amountInCents,
                    description = $"{order.Items.Count} order line(s), including shipping and discounts.",
                    quantity = 1
                }
            };

            var billingData = new
            {
                apartment = order.ShippingApartmentNumber ?? "NA",
                first_name = firstName,
                last_name = lastName,
                street = order.ShippingStreet,
                building = order.ShippingBuildingNumber ?? "NA",
                phone_number = order.ShippingPhoneNumber,
                city = order.ShippingCity,
                country = "EG",
                email = user.Email ?? "NA",
                floor = "NA",
                state = order.ShippingArea
            };

            var payload = new
            {
                amount = amountInCents,
                currency = "EGP",
                payment_methods = _settings.PaymentMethodIds,
                items,
                billing_data = billingData,
                special_reference = $"ORDER-{order.Id}",
                expiration = 3600,
                notification_url = _settings.NotificationUrl,
                redirection_url = BuildRedirectionUrl(order.Id)
            };

            // Claim the order before calling Paymob so concurrent checkout
            // requests cannot create multiple live intentions for one order.
            var paymentClaim = $"creating:{Guid.NewGuid():N}";
            var claimed = await _context.Orders
                .Where(x => x.Id == order.Id &&
                            x.Status == OrderStatus.Confirmed &&
                            ((x.PaymentStatus == PaymentStatus.Pending && x.PaymentIntentId == null && x.PaymobOrderId == null) ||
                             (x.PaymentStatus == PaymentStatus.Failed &&
                              (x.PaymentIntentId == null || !x.PaymentIntentId.StartsWith("creating:")))))
                .ExecuteUpdateAsync(updates => updates
                    .SetProperty(x => x.PaymentIntentId, paymentClaim)
                    .SetProperty(x => x.PaymobOrderId, (long?)null));

            if (claimed != 1)
                throw new BadRequestException(
                    "A payment session is already being created or has already been created for this order.");

            var attempt = new PaymentAttempt
            {
                OrderId = order.Id,
                Status = "Creating",
                AmountCents = amountInCents
            };
            _context.PaymentAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "https://accept.paymob.com/v1/intention/");

            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Token",
                    _settings.SecretKey);

            httpRequest.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            PaymobIntentionResponse? paymobResponse;
            HttpResponseMessage paymobHttpResponse;
            try
            {
                paymobHttpResponse = await _httpClient.SendAsync(httpRequest);
            }
            catch
            {
                attempt.Status = "Unknown";
                attempt.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                throw;
            }

            using (var response = paymobHttpResponse)
            {
                var responseContent =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // A definitive client-side rejection did not create an
                    // intention, so allow a corrected request to be retried.
                    if ((int)response.StatusCode < 500)
                    {
                        attempt.Status = "Rejected";
                        attempt.UpdatedAt = DateTime.UtcNow;
                        await _context.Orders
                            .Where(x => x.Id == order.Id &&
                                        x.PaymentIntentId == paymentClaim)
                            .ExecuteUpdateAsync(updates => updates
                                .SetProperty(x => x.PaymentIntentId, (string?)null));
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        attempt.Status = "Unknown";
                        attempt.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }

                    throw new BadRequestException(
                        "Failed to create Paymob payment intention.");
                }

                paymobResponse =
                    JsonSerializer.Deserialize<PaymobIntentionResponse>(
                        responseContent);

                if (paymobResponse is null ||
                    string.IsNullOrWhiteSpace(paymobResponse.Id) ||
                    string.IsNullOrWhiteSpace(paymobResponse.ClientSecret))
                {
                    attempt.Status = "Unknown";
                    attempt.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    throw new BadRequestException(
                        "Paymob returned an invalid payment intention. Contact support before retrying this order.");
                }
            }

            order.PaymentIntentId = paymobResponse.Id;
            order.PaymobOrderId = paymobResponse.IntentionOrderId;
            order.PaymentStatus = PaymentStatus.Pending;
            attempt.PaymentIntentId = paymobResponse.Id;
            attempt.PaymobOrderId = paymobResponse.IntentionOrderId;
            attempt.Status = "Pending";
            attempt.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new PaymentResponse
            {
                OrderId = order.Id,
                PaymentIntentId = paymobResponse.Id,
                PaymobOrderId = paymobResponse.IntentionOrderId,
                ClientSecret = paymobResponse.ClientSecret
            };
        }

        private string BuildRedirectionUrl(int orderId)
        {
            if (string.IsNullOrWhiteSpace(_settings.RedirectionUrl))
                throw new InvalidOperationException("Paymob redirection URL is not configured.");

            var separator = _settings.RedirectionUrl.Contains("?") ? "&" : "?";
            return $"{_settings.RedirectionUrl}{separator}orderId={Uri.EscapeDataString(orderId.ToString())}";
        }
        private class PaymobIntentionResponse
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("intention_order_id")]
            public long IntentionOrderId { get; set; }

            [JsonPropertyName("client_secret")]
            public string ClientSecret { get; set; } = string.Empty;
        }
    }
}
