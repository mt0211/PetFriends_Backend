using Microsoft.AspNetCore.SignalR;

public class CustomUserIdProvider : IUserIdProvider
{
    private readonly ILogger<CustomUserIdProvider> _logger;

    public CustomUserIdProvider(ILogger<CustomUserIdProvider> logger)
    {
        _logger = logger;
    }

    public string GetUserId(HubConnectionContext connection)
    {
        var userId = connection.User?.FindFirst("userid")?.Value;
        _logger.LogInformation($"GetUserId called for connection {connection.ConnectionId}, returning: {userId}");
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning($"No userId found in claims for connection {connection.ConnectionId}");
            // Liệt kê tất cả claims để debug
            var claims = connection.User?.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
            if (claims != null && claims.Any())
            {
                _logger.LogInformation($"Available claims: {string.Join(", ", claims)}");
            }
            else
            {
                _logger.LogWarning("No claims found in User object");
            }
        }
        
        return userId;
    }
}
