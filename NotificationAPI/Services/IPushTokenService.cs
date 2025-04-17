public interface IPushTokenService
{
        Task<bool> SendPushNotificationAsync(Guid userId, string title, string body, Dictionary<string, string> data = null);
        Task<bool> SendPushNotificationToTokenAsync(string token, string title, string body, Dictionary<string, string> data = null);
}