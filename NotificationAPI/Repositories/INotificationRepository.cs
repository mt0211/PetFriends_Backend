using DataAccess.Models;

public interface INotificationRepository
{
    Task<Pet> GetPetById(Guid petId);
    Task<User> GetUserById(Guid userId);
    Task<Appointment> GetAppointmentById(Guid appointmentId);
    Task<ForumPost> GetForumPostById(Guid forumPostId);
    Task<Promotion> GetPromotionById(Guid promotionId);
    Task<ClinicService> GetClinicServiceById(Guid id);
    Task<Notification> CreateNotification(Notification notification);
    Task<List<Notification>> GetNotifications(Guid userId);
    Task MarkNotificationAsRead(Guid notificationId);
    Task<List<User>> GetListUsers();
    Task<UserPetVaccine> GetUserPetVaccineById(Guid vaccineId);
}