using DataAccess.Models;
using Microsoft.AspNetCore.SignalR;

public class NotificationHub : Hub
{
    private readonly INotificationRepository _repository;
    private readonly ILogger<NotificationHub> _logger;
    public NotificationHub(INotificationRepository repository, ILogger<NotificationHub> logger)

    {
        _logger = logger;
        _repository = repository;
    }

    public async Task SendNotification(NotificationDTO notification)
    {
        if (notification.Metadata != null && notification.Metadata.TryGetValue("userId", out string userId))
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", notification);
        }
        else
        {
            // Nếu không có UserId, có thể log lỗi hoặc gửi đến tất cả (tùy vào yêu cầu)
           // await Clients.All.SendAsync("ReceiveNotification", notification);
           Console.WriteLine("No UserId found in metadAAAAAAAAAAAAAAAAAAAAAAAAAA");
        }
    }
    public override async Task OnConnectedAsync()
    {
            // Khi client kết nối, gửi 10 activities gần nhất
        await base.OnConnectedAsync();
    }
    public async Task<List<Notification>> GetNotifications(Guid userId)
    {
        return await _repository.GetNotifications(userId);
    }

    public async Task MarkAsRead(Guid notificationId)
    {
        await _repository.MarkNotificationAsRead(notificationId);
        _logger?.LogInformation($"Notification {notificationId} marked as read");
    }
}