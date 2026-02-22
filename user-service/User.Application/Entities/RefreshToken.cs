namespace User.Application.Entities
{
    public class RefreshToken
    {
        public long Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRevoked { get; set; }
        public ApplicationUser User { get; set; }
    }
}
