namespace AccessoriesStore.Domain.Entities;

public class SupportConversation
{
    public int Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<SupportMessage> Messages { get; set; } = new List<SupportMessage>();
}
