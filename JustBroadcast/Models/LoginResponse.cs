namespace JustBroadcast.Models
{
    public class LoginResponse
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserInfoDto? User { get; set; }
    }
}
