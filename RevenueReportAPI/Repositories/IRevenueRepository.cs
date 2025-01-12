using DataAccess.Models;

namespace RevenueReportAPI.Repositories
{
    public interface IRevenueRepository
    {
        Task<IEnumerable<dynamic>> GetUserBookingSummaries();
        Task<IEnumerable<dynamic>> GetServiceRevenue(DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<dynamic>> GetTotalRevenue(DateTime? startDate, DateTime? endDate);
    }
}
