using System.Text;
using System.Text.Json;

public class PushTokenService : IPushTokenService
{
    private readonly IPushTokenRepository _pushTokenRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PushTokenService> _logger;

    public PushTokenService(
        IPushTokenRepository pushTokenRepository,
        IHttpClientFactory httpClientFactory,
        ILogger<PushTokenService> logger)
    {
        _pushTokenRepository = pushTokenRepository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<bool> SendPushNotificationAsync(Guid userId, string title, string body, Dictionary<string, string> data = null)
    {
        try
        {
            // Lấy tất cả token của người dùng
            var userTokens = await _pushTokenRepository.GetAllByUserId(userId);

            if (userTokens == null || userTokens.Count == 0)
            {
                _logger.LogInformation($"No push tokens found for user {userId}");
                return false;
            }

            bool anySuccess = false;

            // Gửi thông báo đến tất cả thiết bị của người dùng
            foreach (var userToken in userTokens)
            {
                bool success = await SendPushNotificationToTokenAsync(userToken.Token, title, body, data);
                if (success) anySuccess = true;
            }

            return anySuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending push notification to user {userId}");
            return false;
        }
    }

    public async Task<bool> SendPushNotificationToTokenAsync(string token, string title, string body, Dictionary<string, string> data = null)
    {
        try
        {
            // Chuẩn bị payload cho Expo Push Notification Service
            var pushMessage = new
            {
                to = token,
                title = title,
                body = body,
                data = data ?? new Dictionary<string, string>(),
                sound = "default",
                badge = 1
            };

            var json = JsonSerializer.Serialize(pushMessage);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Gửi request đến Expo Push Notification Service
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync("https://exp.host/--/api/v2/push/send", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Failed to send push notification: {errorResponse}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending push notification to token {token}");
            return false;
        }
    }
}