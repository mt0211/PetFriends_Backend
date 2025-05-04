using Microsoft.AspNetCore.SignalR;

namespace RealTimeCommunicationAPI.Hubs
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        private readonly ILogger<CustomUserIdProvider> _logger;

        public CustomUserIdProvider(ILogger<CustomUserIdProvider> logger)
        {
            _logger = logger;
        }

        public string GetUserId(HubConnectionContext connection)
        {
            // Đảm bảo tìm chính xác claim "userid" như trong token của bạn
            var userId = connection.User?.FindFirst("userid")?.Value;
            
            _logger.LogInformation("GetUserId called for connection {ConnectionId}, returning: {UserId}", 
                connection.ConnectionId, userId);
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("No userId found in claims for connection {ConnectionId}", connection.ConnectionId);
                // Liệt kê tất cả claims để debug
                var claims = connection.User?.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                if (claims != null && claims.Any())
                {
                    _logger.LogInformation("Available claims: {Claims}", string.Join(", ", claims));
                }
                else
                {
                    _logger.LogWarning("No claims found in User object");
                }
            }
            
            return userId ?? string.Empty;
        }
    }
}