using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Models;

namespace RealTimeActivityAPI.Repositories
{
    public interface IRealTimeActivityAPIRepository
    {
        Task<Activity> CreateActivity(Activity activity);
        Task<List<Activity>> GetRecentActivities();
        Task<Appointment> GetAppointmentById(Guid appointmentId);
        Task<Feedback> GetFeedbackById(Guid feedbackId);
        Task<ClinicService> GetClinicServiceById(Guid clinicServiceId);
    }
}