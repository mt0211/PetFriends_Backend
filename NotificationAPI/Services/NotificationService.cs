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
            "APPOINTMENT_CONFIRMATION" => $"🐾 Hey {userName}, your appointment with {petName} on {appointmentDate} has been confirmed! We're looking forward to seeing you and your furry friend. See you soon at PetFriends! 🐕💕",
            "APPOINTMENT_REMINDER_1_HOUR" => $"🐾 Hey {userName}, your appointment with {petName} is just 1 hour away! Make sure to bring their favorite toy or blanket if they get nervous. See you soon at PetFriends! 🐕💕",
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

    //Promotion post notification message
    private string GetPromotionNotificationMessage(string type, Promotion promotion)
    {
        string promotionName = promotion.Name;
        string startDate = promotion.StartDate?.ToString("HH:mm dd/MM/yyyy");
        string endDate = promotion.EndDate?.ToString("HH:mm dd/MM/yyyy");
        string targetGroup = promotion.TargetGroup;
        return type switch
        {
            "PROMOTION_UPDATED" => $"🎉 Hey, we just has a new updated for promotion {promotionName} for with users {targetGroup} , start date {startDate} to end date {endDate}. Let's check it out",
            "PROMOTION_CREATED" => $"🎉 Hey, we just has a new promotion {promotionName} for with users {targetGroup} , start date {startDate} to end date {endDate}. Let's check it out",
            _ => "Updated from promotion"
        };
    }


    //Clinic service notification message
    private string GetClinicServiceNotificationMessage(string type, ClinicService clinicService)
    {
        string clinicServiceName = clinicService.Name;
        string discountFrom = clinicService.DiscountFrom?.ToString("HH:mm dd/MM/yyyy");
        string discountTo = clinicService.DiscountTo?.ToString("HH:mm dd/MM/yyyy");
        return type switch
        {
            "CLINIC_SERVICE_UPDATED" => $"🎉 Hey, we just has a new updated for clinic service {clinicServiceName} with discount from {discountFrom} to {discountTo}. Let's check it out",
            "CLINIC_SERVICE_CREATED" => $"🎉 Hey, we just has a new clinic service {clinicServiceName} with discount from {discountFrom} to {discountTo}. Let's check it out",
            _ => "Updated from clinic service"
        };
    }


    //Vaccine service notification message
    private string GetVaccineNotificationMessage(string type, UserPetVaccine vaccineService, int nextDoseNumber)
    {
        string vaccineName = vaccineService.Name;
        string petName = vaccineService.Pet?.Name ?? "your pet";
        
        return type switch
        {
            "VACCINE_REMINDER" =>
                $"🎉 Hey! Your pet {petName} is due for dose {nextDoseNumber} of the {vaccineName} vaccine. Please bring {petName} in for vaccination.",
                "VACCINE_REMINDER_1_DAY" =>
                $"🎉 Hey! 1 day until your pet {petName} is due for dose {nextDoseNumber} of the {vaccineName} vaccine. Please bring {petName} in for vaccination.",
            _ => string.Empty
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
            "APPOINTMENT_CONFIRMATION" => "Appointment Confirmed!",
            "APPOINTMENT_REMINDER_1_HOUR" => "Appointment Reminder!",
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

    //Promotion notification title
    private string GetPromotionNotificationTitle(string type, Promotion promotion)
    {
        string promotionName = promotion.Name;
        return type switch
        {
            "PROMOTION_UPDATED" => $"New updated for promotion: {promotionName}",
            "PROMOTION_CREATED" => $"New promotion: {promotionName}",
            _ => $"Update from promotion"
        };
    }

    //Clinic service notification title
    private string GetClinicServiceNotificationTitlee(string type, ClinicService clinicService)
    {
        string clinicServiceName = clinicService.Name;
        return type switch
        {
            "CLINIC_SERVICE_UPDATED" => $"New updated for clinic service: {clinicServiceName}",
            "CLINIC_SERVICE_CREATED" => $"New clinic service: {clinicServiceName}",
            _ => $"Update from clinic service"
        };
    }

    //Vaccine service notification title
    private string GetVaccineNotificationTitle(string type, UserPetVaccine vaccineService, int nextDoseNumber)
    {
        string vaccineName = vaccineService.Name;
        string petName = vaccineService.Pet?.Name ?? "your pet";
        
        return type switch
        {
            "VACCINE_REMINDER" => $"Reminder for {petName}'s {nextDoseNumber} dose of {vaccineName} vaccine",
            "VACCINE_REMINDER_1_DAY" => $"Reminder for {petName}'s {nextDoseNumber} dose of {vaccineName} vaccine",
            _ => $"Update from vaccine service"
        };
    }

    public async Task<List<NotificationDTO>> CreatePromotionNotification(string type, Guid promotionId)
    {
        var promotion = await _repository.GetPromotionById(promotionId);
        if (promotion == null) return null;
        
        var userList = await _repository.GetListUsers();
        var notificationDtos = new List<NotificationDTO>();
        
        foreach (var user in userList)
        {
            // Tạo thông báo riêng cho mỗi người dùng
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = user.Id, // Sử dụng ID của người dùng hiện tại
                Type = type,
                Title = GetPromotionNotificationTitle(type, promotion),
                Message = GetPromotionNotificationMessage(type, promotion),
                RelatedEntityId = promotion.Id,
                RelatedEntityType = "Promotion",
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddHours(7),
                ReadAt = null,
                ExpiresAt = DateTime.UtcNow.AddHours(2)
            };
            
            // Lưu thông báo vào database
            var createdNotification = await _repository.CreateNotification(notification);
            
            // Chuyển đổi thành DTO
            var notificationDto = MapToDTO(createdNotification);
            notificationDto.Metadata.Add("userId", notification.UserId.ToString());
            notificationDtos.Add(notificationDto);
            
            // Gửi thông báo qua SignalR đến đúng người dùng
            await _hubContext.Clients.User(user.Id.ToString()).SendAsync("ReceiveNotification", notificationDto);
            
            // Gửi push notification
            await _pushTokenService.SendPushNotificationAsync(
                user.Id,
                notification.Title,
                notification.Message,
                new Dictionary<string, string>
                {
                    { "type", type },
                    { "promotionId", promotionId.ToString() },
                    { "notificationId", notification.Id.ToString() }
                }
            );
        }
        
        Console.WriteLine($"Promotion notifications created and sent to {userList.Count} users");
        return notificationDtos;
    }

    public async Task<List<NotificationDTO>> CreateClinicServiceNotification(string type, Guid clinicServiceId)
    {
        var clinicservice = await _repository.GetClinicServiceById(clinicServiceId);
        if (clinicservice == null) return null;
        var user = await _repository.GetListUsers();
         var notificationDtos = new List<NotificationDTO>();
         foreach (var userlist in user)
         {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userlist.Id, // Sử dụng ID của người dùng hiện tại
                Type = type,
                Title = GetClinicServiceNotificationTitlee(type, clinicservice),
                Message = GetClinicServiceNotificationMessage(type, clinicservice),
                RelatedEntityId = clinicservice.Id,
                RelatedEntityType = "ClinicService",
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddHours(7),
                ReadAt = null,
                ExpiresAt = DateTime.UtcNow.AddHours(2)
                
            };
             var createdNotification = await _repository.CreateNotification(notification);
              var notificationDto = MapToDTO(createdNotification);
            notificationDto.Metadata.Add("userId", notification.UserId.ToString());
            notificationDtos.Add(notificationDto);
            await _hubContext.Clients.User(userlist.Id.ToString()).SendAsync("ReceiveNotification", notificationDto);
             await _pushTokenService.SendPushNotificationAsync(
                userlist.Id,
                notification.Title,
                notification.Message,
                new Dictionary<string, string>
                {
                    { "type", type },
                    { "clinicserviceId", clinicServiceId.ToString() },
                    { "notificationId", notification.Id.ToString() }
                }
            );
         }
          Console.WriteLine($"Clinicservice notifications created and sent to {user.Count} users");
        return notificationDtos;
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

    public async Task<NotificationDTO> CreateVaccineNotification(string type, Guid vaccineId)
    {
        var vaccine = await _repository.GetUserPetVaccineById(vaccineId);
        if (vaccine == null || vaccine.Pet == null) return null;
        
        // Tính toán liều tiêm tiếp theo
        var lastDose = vaccine.UserPetVaccineDoses
            .OrderByDescending(d => d.DoseNumber)
            .FirstOrDefault();
            
        if (lastDose == null) return null;
        
        int nextDoseNumber = (lastDose.DoseNumber ?? 0) + 1;
        
        // Kiểm tra xem đã tiêm đủ liều chưa
        if (nextDoseNumber > vaccine.NumberOfDoses) return null;
        
        // Lấy UserId từ Pet
        var userId = vaccine.Pet.UserId;
        if (!userId.HasValue) return null;
        
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId.Value,
            Type = type,
            Title = GetVaccineNotificationTitle(type, vaccine, nextDoseNumber),
            Message = GetVaccineNotificationMessage(type, vaccine, nextDoseNumber),
            RelatedEntityId = vaccine.Id,
            RelatedEntityType = "Vaccine",
            IsRead = false,
            CreatedAt = DateTime.UtcNow.AddHours(7),
            ReadAt = null,
            ExpiresAt = DateTime.UtcNow.AddDays(2)
        };
        
        Console.WriteLine("Notification created successfully");
        var createdNotification = await _repository.CreateNotification(notification);
        var notificationDto = MapToDTO(createdNotification);
        notificationDto.Metadata.Add("userId", notification.UserId.ToString());
        
        await _hubContext.Clients.User(notification.UserId.ToString()).SendAsync("ReceiveNotification", notificationDto);
        
        await _pushTokenService.SendPushNotificationAsync(
            notification.UserId,
            notification.Title,
            notification.Message,
            new Dictionary<string, string>
            {
                { "type", type },
                { "vaccineId", vaccineId.ToString() },
                { "notificationId", notification.Id.ToString() },
                { "nextDoseNumber", nextDoseNumber.ToString() }
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