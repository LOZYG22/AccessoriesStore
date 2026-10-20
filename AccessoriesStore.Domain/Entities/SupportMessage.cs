namespace AccessoriesStore.Domain.Entities;

public class SupportMessage
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public SupportConversation Conversation { get; set; } = null!;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsReadByCustomer { get; set; }
    public bool IsReadByAdmin { get; set; }
}
