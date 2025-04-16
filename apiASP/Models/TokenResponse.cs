namespace apiASP.Models
{
    public class TokenResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
    }
} 