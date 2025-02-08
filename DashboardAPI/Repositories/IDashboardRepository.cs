using DataAccess.Models;

namespace DashboardAPI.Repositories
{
    public interface IDashboardRepository
    {
        Task<(int userCount, int postCount, int serviceCount)> GetDataCount();
        Task<(int pending, int approved, int rejected)> GetForumPostStatistic(DateTime? date = null);
        Task<User> GetUserByID(Guid id);
    }
}
