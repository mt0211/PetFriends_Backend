using DataAccess.Models;

namespace RevenueReportAPI.Repositories
{
    public interface IRevenueRepository
    {
        Task<IEnumerable<dynamic>> GetUserBookingSummaries();
        Task<IEnumerable<dynamic>> GetDetailServiceRevenue(DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<dynamic>> GetServiceRevenue(DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<dynamic>> GetTotalRevenue(int year, int? month);
        Task<List<dynamic>> GetUserBookingSummariesForExport();
        Task<List<dynamic>> GetRevenueForExport(int year, int? month);
        Task<(List<dynamic> userbookingsummaries, List<dynamic> revenues, List<dynamic> servicerevenue)> GetAllDataForExport(int year, int? month, DateTime? startDate, DateTime? endDate);
    }
}
