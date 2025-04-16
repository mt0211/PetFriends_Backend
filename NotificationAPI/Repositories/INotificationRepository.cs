using DataAccess.Models;

public interface INotificationRepository
{
    Task<Pet> GetPetById(Guid petId);
    Task<User> GetUserById(Guid userId);
    Task<Appointment> GetAppointmentById(Guid appointmentId);
    Task<ForumPost> GetForumPostById(Guid forumPostId);
    Task<Notification> CreateNotification(Notification notification);
    Task<List<Notification>> GetNotifications(Guid userId);
    Task MarkNotificationAsRead(Guid notificationId);
}