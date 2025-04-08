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

        public async Task<List<Activity>> GetRecentActivities(int count = 10)
        {
            return await _context.Activities
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
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
        
    }
}