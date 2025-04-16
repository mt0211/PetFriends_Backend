using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

public class NotificationRepository : INotificationRepository
{
    private readonly PetfriendsContext _context;
    public NotificationRepository(PetfriendsContext context)
    {
        _context = context;
    }

    public async Task<Pet> GetPetById(Guid petId)
    {
        return await _context.Pets
        .FirstOrDefaultAsync(p => p.Id == petId);
    }

    public async Task<User> GetUserById(Guid userId)
    {
        return await _context.Users
        .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<Appointment> GetAppointmentById(Guid appointmentId)
    {
        return await _context.Appointments
        .Include(a => a.User)
        .Include(a => a.Pet)
        .FirstOrDefaultAsync(u => u.Id == appointmentId);
    }

    public async Task<ForumPost> GetForumPostById(Guid forumPostId)
    {
        return await _context.ForumPosts
        .Include(f => f.User)
        .FirstOrDefaultAsync(u => u.Id == forumPostId);
    }

    public async Task<Notification> CreateNotification(Notification notification)
    {
        notification.Id = Guid.NewGuid();
        notification.CreatedAt = DateTime.UtcNow.AddHours(7);
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<List<Notification>> GetNotifications(Guid userId)
    {
        return await _context.Notifications
        .Where(n => n.UserId == userId)
        .OrderByDescending(n => n.CreatedAt)
        .ToListAsync();
    }
    public async Task MarkNotificationAsRead(Guid notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow.AddHours(7);
            await _context.SaveChangesAsync();
        }
    }
}