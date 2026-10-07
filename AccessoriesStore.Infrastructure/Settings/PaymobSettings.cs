
namespace AccessoriesStore.Infrastructure.Settings
{
    public class PaymobSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string HmacSecret { get; set; } = string.Empty;
        public string NotificationUrl { get; set; } = string.Empty;

        public List<int> PaymentMethodIds { get; set; } = new();
    }
}
