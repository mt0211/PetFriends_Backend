public interface INotificationService
{
    Task<NotificationDTO> CreatePetBirthdayNotification(string type, Guid petId);
     Task<NotificationDTO> CreateUserBirthdayNotification(string type, Guid userId);
     Task<NotificationDTO> CreateAppointmentNotification(string type, Guid appointmentId);
     Task<NotificationDTO> CreateForumPostNotification(string type, Guid postId, Guid reactingUserId, Guid postOwnerId);
     Task<List<NotificationDTO>> CreatePromotionNotification(string type, Guid promotionId);
     Task<List<NotificationDTO>> CreateClinicServiceNotification(string type, Guid clinicServiceId);
     Task<NotificationDTO> CreateVaccineNotification(string type, Guid vaccineId);
}