namespace AccessoriesStore.Domain.Entities;

public class PaymentAttempt
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Status { get; set; } = "Creating";
    public string? PaymentIntentId { get; set; }
    public long? PaymobOrderId { get; set; }
    public long? PaymobTransactionId { get; set; }
    public long AmountCents { get; set; }
    public long RefundedAmountCents { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
