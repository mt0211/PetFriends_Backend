namespace AppUserAuthenticationAPI.Helpers;

public class AppConfig
    {
        public JwtSettings JwtSettings { get; set; }
    }
    public class JwtSettings
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
    }
    public class GoogleOAuthOptions
    {
        public List<string> AllowedClientIds { get; set; }
    }