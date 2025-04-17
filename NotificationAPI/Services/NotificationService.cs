using DataAccess.Models;
using Microsoft.AspNetCore.SignalR;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IPushTokenService _pushTokenService;
    private readonly IHubContext<NotificationHub> _hubContext;
    public NotificationService(INotificationRepository repository, IHubContext<NotificationHub> hubContext, IPushTokenService pushTokenService)
    {
        _repository = repository;
        _hubContext = hubContext;
        _pushTokenService = pushTokenService;
    }

    private NotificationDTO MapToDTO(Notification notification)
    {
        var metadata = new Dictionary<string, string>();
        
        if (notification.RelatedEntityId.HasValue && !string.IsNullOrEmpty(notification.RelatedEntityType))
        {
            metadata.Add("entityId", notification.RelatedEntityId.Value.ToString());
            metadata.Add("entityType", notification.RelatedEntityType);
        }
        
        return new NotificationDTO
        {
            Id = notification.Id,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            RelatedEntityId = notification.RelatedEntityId,
            RelatedEntityType = notification.RelatedEntityType,
            Metadata = metadata
        };
    }
    
    //Pet birthday notification message
    private string GetPetBirthdayNotificationMessage(string type, Pet pet)
    {
        string petName = pet.Name;
        return type switch
        {
            "PET_BIRTHDAY" => $"🎉 Happy Birthday, {petName}! Wishing your furry friend a day full of belly rubs, tasty treats, and joyful tail wags. From all of us at PetFriends, thank you for letting us be part of {petName}'s journey. Here's to more healthy, happy years ahead! 🐾💙",
            _ => $"Updated from pet {petName}"
        };      
    }

    //User birthday notification message
    public string GetUserBirthdayNotificationMessage(string type, User user)
    {
        string userName = user.FullName;
        return type switch
        {
            "USER_BIRTHDAY" => $"🎉 Happy Birthday, {userName}! Wishing you a day filled with love, laughter, and all the things that make you smile. From all of us at PetFriends, thank you for being a part of our community. Here's to many more years of joy, happiness, and furry friends! 💙🎂",
            _ => $"Updated from user {userName}"
        };
    }

    //Appointment notification message
    public string GetAppointmentNotificationMessage(string type, Appointment appointment)
    {
        string appointmentDate = appointment.CreatedAt?.ToString("HH:mm dd/MM/yyyy");
        string userName = appointment.User?.FullName ?? "Unknown";
        string petName = appointment.Pet?.Name ?? "Unknown pet";
        return type switch
        {
            "APPOINTMENT_REVIEW_REMINDER" => $"🐾 Hey {userName}, just a reminder that your appointment with {petName} on {appointmentDate} has been completed! We'd love to hear your feedback so we can continue improving and make your experience even better 🐕💕",
            "APPOINTMENT_REMINDER" => $"🐾 Hey {userName}, just a reminder that you and your furry friend {petName} have an appointment coming up within 24 hours! Make sure to bring their favorite toy or blanket if they get nervous. See you soon at PetFriends! 🐕💕",
            _ => $"Updated from appointment",
        };
    }

    //Forum post notification message
    private string GetPostInteractionMessage(string type, User reactingUser, ForumPost post)
    {
        string userName = reactingUser?.FullName ?? "Someone";
        string truncatedContent = TruncateContent(post?.PostContent ?? "your post", 50);
        string time = DateTime.UtcNow.AddHours(7).ToString("HH:mm dd/MM/yyyy");
        
        return type switch
        {
            "POST_LIKE" => $"👍 {userName} liked your post: \"{truncatedContent}\" at {time}",
            "POST_DISLIKE" => $"👎 {userName} disliked your post: \"{truncatedContent}\" at {time}",
            "POST_COMMENT" => $"💬 {userName} commented on your post: \"{truncatedContent}\" at {time}",
            _ => $"There's new activity on your post: \"{truncatedContent}\" at {time}"
        };
    }

    // Helper method để cắt ngắn nội dung bài viết
    private string TruncateContent(string content, int maxLength)
    {
        if (string.IsNullOrEmpty(content)) return string.Empty;
        if (content.Length <= maxLength) return content;
        return content.Substring(0, maxLength) + "...";
    }

    //Pet birthday notification title
    private string GetPetBirthdayNotificationTitle(string type, Pet pet)
    {
        string petName = pet.Name;
        return type switch
        {
            "PET_BIRTHDAY" => $"Today is {petName}'s birthday!",
            _ => $"Update from pet {petName}"
        };
    }

    //User birthday notification title
    private string GetUserBirthdayNotificationTitle(string type, User user)
    {
        string userName = user.FullName;
        return type switch
        {
            "USER_BIRTHDAY" => $"Happy Birthday, {userName}!",
            _ => $"Update from user {userName}"
        };    
    }

    //Appointment notification title
    private string GetAppointmentNotificationTitle(string type, Appointment appointment)
    {
        return type switch
        {
            "APPOINTMENT_REVIEW_REMINDER" => "Appointment Review Reminder!",
            "APPOINTMENT_REMINDER" => "Appointment Reminder!",
        _ => $"Update from appointment"
        };
    }

    //Forum post notification title
   private string GetPostInteractionTitle(string type, User reactingUser, ForumPost post)
{
    string userName = reactingUser?.FullName ?? "Someone";
    string truncatedContent = TruncateContent(post?.PostContent ?? "your post", 30);
    
    return type switch
    {
        "POST_LIKE" => $"{userName} liked your post",
        "POST_DISLIKE" => $"{userName} disliked your post",
        "POST_COMMENT" => $"{userName} commented on your post",
        _ => $"New activity on your post"
    };
}
    public async Task<NotificationDTO> CreatePetBirthdayNotification(string type, Guid petId)
    {
        var pet = await _repository.GetPetById(petId);
        if (pet == null) return null;
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = pet.UserId.GetValueOrDefault(),
            Type = type,
            Title = GetPetBirthdayNotificationTitle(type, pet),
            Message = GetPetBirthdayNotificationMessage(type, pet),
            RelatedEntityId = pet.Id,
            RelatedEntityType = "Pet",
            IsRead = false,
            CreatedAt = DateTime.UtcNow.AddHours(7),
            ReadAt = null,
            ExpiresAt = DateTime.UtcNow.AddDays(2)
        };
        Console.WriteLine("Notification created successfullyYYYYYYYYYYYYYYYYYYYYYYYYYYY");
        var CreateNotification = await _repository.CreateNotification(notification);
        var notificationDto = MapToDTO(CreateNotification);
        notificationDto.Metadata.Add("userId", notification.UserId.ToString());
        
        await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("ReceiveNotification", notificationDto);

        await _pushTokenService.SendPushNotificationAsync(
            notification.UserId,
            notification.Title,
            notification.Message,
            new Dictionary<string, string> 
            { 
                { "type", type }, 
                { "petId", petId.ToString() },
                { "notificationId", notification.Id.ToString() }
            }
        );

        return notificationDto;
    }

    public async Task<NotificationDTO> CreateUserBirthdayNotification(string type, Guid userId)
    {
        var user = await _repository.GetUserById(userId);
        if (user == null) return null;
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Type = type,
            Title = GetUserBirthdayNotificationTitle(type, user),
            Message = GetUserBirthdayNotificationMessage(type, user),
            RelatedEntityId = user.Id,
            RelatedEntityType = "User",
            IsRead = false,
            CreatedAt = DateTime.UtcNow.AddHours(7),
            ReadAt = null,
            ExpiresAt = DateTime.UtcNow.AddDays(2)
        };
        Console.WriteLine("Notification created successfullyYYYYYYYYYYYYYYYYYYYYYYYYYYY");
        var CreateNotification = await _repository.CreateNotification(notification);
        var notificationDto = MapToDTO(CreateNotification);
        notificationDto.Metadata.Add("userId", notification.UserId.ToString());
          await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("ReceiveNotification", notificationDto);

         await _pushTokenService.SendPushNotificationAsync(
            notification.UserId,
            notification.Title,
            notification.Message,
            new Dictionary<string, string> 
            { 
                { "type", type }, 
                { "userId", userId.ToString() },
                { "notificationId", notification.Id.ToString() }
            }
        );

        return notificationDto;
    }

    public async Task<NotificationDTO> CreateAppointmentNotification(string type, Guid appointmentId)
    {
        var appointment = await _repository.GetAppointmentById(appointmentId);
        if (appointment == null) return null;
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = appointment.UserId.Value,
            Type = type,
            Title = GetAppointmentNotificationTitle(type, appointment),
            Message = GetAppointmentNotificationMessage(type, appointment),
            RelatedEntityId = appointment.Id,
            RelatedEntityType = "Appointment",
            IsRead = false,
            CreatedAt = DateTime.UtcNow.AddHours(7),
            ReadAt = null,
            ExpiresAt = DateTime.UtcNow.AddDays(2)
        };
        Console.WriteLine("Notification created successfullyYYYYYYYYYYYYYYYYYYYYYYYYYYY");
        var CreateNotification = await _repository.CreateNotification(notification);
        var notificationDto = MapToDTO(CreateNotification);
        notificationDto.Metadata.Add("userId", notification.UserId.ToString());
        await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("ReceiveNotification", notificationDto);

        await _pushTokenService.SendPushNotificationAsync(
            notification.UserId,
            notification.Title,
            notification.Message,
            new Dictionary<string, string>
            {
                { "type", type },
                { "appointmentId", appointmentId.ToString() },
                { "notificationId", notification.Id.ToString() }
            }
        );

        return notificationDto;
    }

    public async Task<NotificationDTO> CreateForumPostNotification(string type, Guid postId, Guid reactingUserId, Guid postOwnerId)
    {
       // Lấy thông tin bài viết
        var post = await _repository.GetForumPostById(postId);
        if (post == null) return null;
        
        // Lấy thông tin người tương tác
        var reactingUser = await _repository.GetUserById(reactingUserId);
        if (reactingUser == null) return null;
        
        // Tạo tiêu đề và nội dung thông báo
        string title = GetPostInteractionTitle(type, reactingUser, post);
        string message = GetPostInteractionMessage(type, reactingUser, post);
        
        // Tạo thông báo
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = postOwnerId,
            Type = type,
            Title = title,
            Message = message,
            RelatedEntityId = postId,
            RelatedEntityType = "ForumPost",
            IsRead = false,
            CreatedAt = DateTime.UtcNow.AddHours(7),
            ReadAt = null,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        
        Console.WriteLine($"Creating notification for post interaction: {type}");
        var createdNotification = await _repository.CreateNotification(notification);
        var notificationDto = MapToDTO(createdNotification);
        notificationDto.Metadata.Add("userId", notification.UserId.ToString());
        notificationDto.Metadata.Add("reactingUserId", reactingUserId.ToString());
        
        // Gửi thông báo realtime qua SignalR
        await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("ReceiveNotification", notificationDto);

        await _pushTokenService.SendPushNotificationAsync(
            notification.UserId,
            notification.Title,
            notification.Message,
            new Dictionary<string, string>
            {
                { "type", type },
                { "postId", postId.ToString() },
                { "notificationId", notification.Id.ToString() }
            }
        );
        return notificationDto;
    }

}