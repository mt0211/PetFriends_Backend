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
    }
}