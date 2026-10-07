namespace AccessoriesStore.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public string TokenHash { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedAt { get; set; }

        public bool IsRevoked => RevokedAt.HasValue;

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    }
}
