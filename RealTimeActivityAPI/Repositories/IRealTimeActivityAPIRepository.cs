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
        Task<List<Activity>> GetRecentActivities(int count = 10);
        Task<Appointment> GetAppointmentById(Guid appointmentId);
    }
}