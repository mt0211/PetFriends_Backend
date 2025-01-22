using DashboardAPI.DTOs.ResultModel;

namespace DashboardAPI.Services
{
    public interface IDashboardService
    {
        Task<ResultModel> GetData(string token);
    }
}
