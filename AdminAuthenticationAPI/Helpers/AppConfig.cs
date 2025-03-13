namespace AdminAuthenticationAPI.Helpers;
public class AppConfig
{
    public JwtSettings JwtSettings { get; set; }
}
public class JwtSettings
{
    public string SecretKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int AccessTokenExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; }
}
 public class GoogleOAuthOptions
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }
