using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealTimeActivityAPI.Repositories
{
    public class RealTimeActivityAPIRepository : IRealTimeActivityAPIRepository
    {
        private readonly PetfriendsContext _context;

        public RealTimeActivityAPIRepository(PetfriendsContext context)
        {
            _context = context;
        }
        public async Task<Activity> CreateActivity(Activity activity)
        {
            activity.Id = Guid.NewGuid();
            activity.CreatedAt = DateTime.UtcNow;
            await _context.Activities.AddAsync(activity);
            await _context.SaveChangesAsync();
            return activity;
        }

        public async Task<List<Activity>> GetRecentActivities()
        {
            return await _context.Activities
                .Where(a => a.Type == "APP_APPOINTMENT_CANCELED"
                || a.Type == "APP_APPOINTMENT_CREATED"
                || a.Type == "APPOINTMENT_CANCELLED"
                || a.Type == "APPOINTMENT_COMPLETED"
                || a.Type == "APPOINTMENT_CONFIRMED"
                || a.Type == "APPOINTMENT_CREATED"
                || a.Type == "CLINIC_SERVICE_CREATED"
                || a.Type == "DISCOUNT_ENDED"
                || a.Type == "FEEDBACK_RECEIVED"
                || a.Type == "PROMOTION_CREATED"
                || a.Type == "PROMOTION_EXPIRED"
                )
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Activity>> AdminGetRecentActivities()
        {
            return await _context.Activities
            .Where(a => a.Type == "CLINIC_SERVICE_CREATED"
            || a.Type == "USER_CREATED"
             || a.Type == "APP_USER_CREATED"
             || a.Type == "POST_CREATED")
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
        }
        public async Task<Appointment> GetAppointmentById(Guid appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Pet)
                .Include(a => a.GuestUser)
                .Include(a => a.GuestPet)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
        }
        public async Task<Feedback> GetFeedbackById(Guid feedbackId)
        {
            return await _context.Feedbacks
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == feedbackId);
        }

        public async Task<ClinicService> GetClinicServiceById(Guid clinicServiceId)
        {
            return await _context.ClinicServices
            .FirstOrDefaultAsync(a => a.Id == clinicServiceId);
        }

        public async Task<Promotion> GetPromotionById(Guid promotionId)
        {
            return await _context.Promotions
            .FirstOrDefaultAsync(a => a.Id == promotionId);
        }
        
        public async Task<User> GetUserById(Guid userId)
        {
            return await _context.Users
            .FirstOrDefaultAsync(a => a.Id == userId);
        }
        
        public async Task<ForumPost> GetForumPostById(Guid id)
        {
            return await _context.ForumPosts
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);
        }
        
    }
}