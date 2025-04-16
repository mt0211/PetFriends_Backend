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
            _logger?.LogInformation($"Sending notification to user {userId}");
            await Clients.User(userId).SendAsync("ReceiveNotification", notification);
            _logger?.LogInformation($"Notification sent to user {userId}");
        }
        else
        {
            _logger?.LogWarning("No UserId found in metadata");
            Console.WriteLine("No UserId found in metadata");
        }
    }
    
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        _logger?.LogInformation($"Client connected: {Context.ConnectionId}, UserId: {userId}");
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.UserIdentifier;
        _logger?.LogInformation($"Client disconnected: {Context.ConnectionId}, UserId: {userId}, Exception: {exception?.Message}");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task<List<Notification>> GetNotifications(Guid userId)
    {
        _logger?.LogInformation($"Getting notifications for user {userId}");
        var notifications = await _repository.GetNotifications(userId);
        _logger?.LogInformation($"Retrieved {notifications.Count} notifications for user {userId}");
        return notifications;
    }

    public async Task MarkAsRead(Guid notificationId)
    {
        await _repository.MarkNotificationAsRead(notificationId);
        _logger?.LogInformation($"Notification {notificationId} marked as read");
    }
}
