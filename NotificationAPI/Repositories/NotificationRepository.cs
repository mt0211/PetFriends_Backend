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

    public async Task<Promotion> GetPromotionById(Guid promotionId)
    {
        return await _context.Promotions
        .FirstOrDefaultAsync(u => u.Id == promotionId);
    }

    public async Task<UserPetVaccine> GetUserPetVaccineById(Guid vaccineId)
    {
        return await _context.UserPetVaccines
            .Include(upv => upv.Pet)
                .ThenInclude(p => p.User)
            .Include(upv => upv.UserPetVaccineDoses)
            .Include(upv => upv.Vaccine)
                .ThenInclude(v => v.VaccineDoses)
            .FirstOrDefaultAsync(upv => upv.Id == vaccineId);
    }

    public async Task<(DateTime? NextDoseDate, int? NextDoseNumber)> CalculateNextDoseInfo(Guid userPetVaccineId)
    {
        var userPetVaccine = await GetUserPetVaccineById(userPetVaccineId);
        if (userPetVaccine == null)
            return (null, null);
            
        // Lấy liều tiêm cuối cùng
        var lastDose = userPetVaccine.UserPetVaccineDoses
            .OrderByDescending(d => d.DoseNumber)
            .FirstOrDefault();
            
        if (lastDose == null || !lastDose.DateGiven.HasValue)
            return (null, null);
            
        // Kiểm tra xem đã tiêm đủ liều chưa
        if (lastDose.DoseNumber >= userPetVaccine.NumberOfDoses)
            return (null, null);
            
        int nextDoseNumber = (lastDose.DoseNumber ?? 0) + 1;
        DateTime? nextDoseDate = null;
        
        // Nếu là vaccine hệ thống
        if (userPetVaccine.VaccineId.HasValue && userPetVaccine.Vaccine != null)
        {
            var nextDoseInfo = userPetVaccine.Vaccine.VaccineDoses
                .FirstOrDefault(vd => vd.DoseNumber == nextDoseNumber);
                
            if (nextDoseInfo != null && nextDoseInfo.DaysAfterPrevious.HasValue)
            {
                nextDoseDate = lastDose.DateGiven.Value.AddDays(nextDoseInfo.DaysAfterPrevious.Value);
            }
        }
        else
        {
            // Nếu là vaccine tự thêm, giả sử khoảng cách giữa các liều là 30 ngày
            nextDoseDate = lastDose.DateGiven.Value.AddDays(30);
        }
        
        return (nextDoseDate, nextDoseNumber);
    }

    public async Task<List<User>> GetListUsers()
    {
        return await _context.Users
        .Where(u => u.Role == "USER")
        .ToListAsync();
    }

    public async Task<ClinicService> GetClinicServiceById(Guid id)
    {
        return await _context.ClinicServices
        .FirstOrDefaultAsync(u => u.Id == id);
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