using DataAccess.Models;

namespace ClinicDasboardAPI.Repositories
{
    public interface IClinicDashboardRepository
    {
        Task<(int userCount, int petCount, decimal totalRevenue, float avgRating)> GetDataCount();
        Task<User> GetUserByID(Guid id);
        Task<(int pending, int confirmed, int completed, int canceled)> GetAppointmentStatistic(DateTime? date);
    }
}
