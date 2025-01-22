using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DashboardAPI.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly PetfriendsContext _context;
        public DashboardRepository(PetfriendsContext context) 
        {
            _context = context;
        }
        public async Task<(int userCount, int petCount, decimal totalRevenue, int postCount, int serviceCount)> GetDataCount()
        {
            var userCount = await _context.Users.CountAsync();
            var petCount = await _context.Pets.CountAsync();
            var postCount = await _context.ForumPosts.CountAsync();
            var serviceCount = await _context.ClinicServices.CountAsync();
            var today = DateTime.Today;
            var startOfMonth = new DateOnly(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var totalRevenue = await _context.DailyRevenueSummaries
                .Where(r => r.Date >= startOfMonth && r.Date <= endOfMonth)
                .SumAsync(r => r.TotalRevenue);
            return (userCount, petCount, totalRevenue, postCount, serviceCount);
        }
    }
}
