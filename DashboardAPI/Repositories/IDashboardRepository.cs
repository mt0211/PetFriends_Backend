namespace DashboardAPI.Repositories
{
    public interface IDashboardRepository
    {
        Task<(int userCount, int petCount, decimal totalRevenue, int postCount, int serviceCount)> GetDataCount();
    }
}
