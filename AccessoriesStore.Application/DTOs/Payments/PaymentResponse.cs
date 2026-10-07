
namespace AccessoriesStore.Application.DTOs.Payments
{
    public class PaymentResponse
    {
        public int OrderId { get; set; }
        public string PaymentIntentId { get; set; } = string.Empty;
        public long PaymobOrderId { get; set; }
        public string ClientSecret { get; set; } = string.Empty;
    }
}
