using AccessoriesStore.Application.Abstractions.Payments;
using AccessoriesStore.Application.DTOs.Payments;
using AccessoriesStore.Domain.Enums;
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

            if (order.PaymentStatus != PaymentStatus.Pending)
                throw new BadRequestException(
                    "Order is not available for payment.");

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

            var items = order.Items.Select(item => new
            {
                name = item.ProductName,
                amount = (long)Math.Round(
                    item.UnitPrice * 100,
                    MidpointRounding.AwayFromZero),
                description = item.ProductName,
                quantity = item.Quantity
            }).ToList();

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
                notification_url = _settings.NotificationUrl
            };

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

            using var response =
                await _httpClient.SendAsync(httpRequest);

            var responseContent =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new BadRequestException(
                    "Failed to create Paymob payment intention.");
            }

            var paymobResponse =
                JsonSerializer.Deserialize<PaymobIntentionResponse>(
                    responseContent);

            if (paymobResponse is null ||
                string.IsNullOrWhiteSpace(paymobResponse.Id) ||
                string.IsNullOrWhiteSpace(paymobResponse.ClientSecret))
            {
                throw new BadRequestException(
                    "Invalid response received from Paymob.");
            }

            order.PaymentIntentId = paymobResponse.Id;
            order.PaymobOrderId = paymobResponse.IntentionOrderId;

            await _context.SaveChangesAsync();

            return new PaymentResponse
            {
                OrderId = order.Id,
                PaymentIntentId = paymobResponse.Id,
                PaymobOrderId = paymobResponse.IntentionOrderId,
                ClientSecret = paymobResponse.ClientSecret
            };
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
