using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicDasboardAPI.Repositories
{
    public class ClinicDashboardRepository : IClinicDashboardRepository
    {
        private readonly PetfriendsContext _context;
        public ClinicDashboardRepository(PetfriendsContext context)
        {
            _context = context;
        }
        public async Task<(int userCount, int petCount, decimal totalRevenue, float avgRating)> GetDataCount()
        {
            var userCount = await _context.Users.CountAsync();
            var petCount = await _context.Pets.CountAsync();
            var today = DateTime.Today;
            var startOfMonth = new DateOnly(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var totalRevenue = await _context.DailyRevenueSummaries
                .Where(r => r.Date >= startOfMonth && r.Date <= endOfMonth)
                .SumAsync(r => r.TotalRevenue);
            var averageRating = await _context.Feedbacks
             .Where(x => x.Rating != null)
             .AverageAsync(x => (float?)x.Rating) ?? 0;
            return (userCount, petCount, totalRevenue, averageRating);
        }
        public async Task<User> GetUserByID(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return null;
            }
            return user;
        }

        public async Task<(int pending, int confirmed, int completed, int canceled)> GetAppointmentStatistic(DateTime? date)
        {
            var selectedDate = date ?? DateTime.Today; 
            var query = _context.Appointments
                .Where(x => EF.Functions.DateDiffDay(x.CreatedAt, selectedDate) == 0); 
            var pendingCount = await query.CountAsync(x => x.Status == "Pending");
            var confirmedCount = await query.CountAsync(x => x.Status == "Confirmed");
            var canceledCount = await query.CountAsync(x => x.Status == "Canceled");
            var completedCount = await query.CountAsync(x => x.Status == "Completed");
            return (pendingCount, confirmedCount, completedCount, canceledCount);
        }
    }
}
