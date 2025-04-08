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
        Task<List<ActivityDTO>> GetRecentActivities(int count = 10);
    }
}