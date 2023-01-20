namespace ECommerce1.Models
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public string AppUserId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public AuthUser AuthUser { get; set; }
    }
}
