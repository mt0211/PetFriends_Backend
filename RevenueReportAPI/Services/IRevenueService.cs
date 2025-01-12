using RevenueReportAPI.DTOs.ResultModel;

namespace RevenueReportAPI.Services
{
    public interface IRevenueService
    {
        Task<ResultModel> GetUserBookingSummary(string token);
        Task<ResultModel> GetServiceRevenue(string token, RevenueRequestModel request);
        Task<ResultModel> GetTotalRevenue(string token, RevenueRequestModel request);
    }
}
