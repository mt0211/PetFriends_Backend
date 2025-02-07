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

        public async Task<(int pending, int approved, int rejected)> GetForumPostStatistic(DateTime? date )
        {
                DateTime selectedDate = (date ?? DateTime.UtcNow).Date;
                date = selectedDate.Date;
            var counts = await _context.ForumPosts
                .Where(p => p.CreatedAt.HasValue && p.CreatedAt.Value.Date == date)
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
            int pendingCount = 0, approvedCount = 0, rejectedCount = 0;
            counts.TryGetValue(0, out pendingCount);
            counts.TryGetValue(1, out approvedCount);
            counts.TryGetValue(2, out rejectedCount);
            return (pendingCount, approvedCount, rejectedCount);
        }
        public async Task<User> GetUserByID(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u=> u.Id == id);
            if (user == null)
            {
                return null;
            }
            return user;
        }

    }
}
