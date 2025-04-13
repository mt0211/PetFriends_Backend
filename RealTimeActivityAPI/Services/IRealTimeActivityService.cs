using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RealTimeActivityAPI.DTOs;

namespace RealTimeActivityAPI.Services
{
    public interface IRealTimeActivityService
    {
        Task<ActivityDTO> CreateAppointmentActivity(string type, Guid appointmentId);
        Task<ActivityDTO> CreateFeedbackActivity(string type, Guid feedbackId);
        Task<List<ActivityDTO>> GetRecentActivities();
        Task<ActivityDTO> CreateClinicServiceActivity(string type, Guid clinicServiceId);
        Task<ActivityDTO> CreatePromotionActivity(string type, Guid promotionId);
        Task<ActivityDTO> CreateUserActivity(string type, Guid userId);
        Task<ActivityDTO> CreatePostActivity(string type, Guid postId);
    }
}