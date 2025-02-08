using ClinicDasboardAPI.DTOs.ResultModel;

namespace ClinicDasboardAPI.Services
{
    public interface IClinicDashboardService
    {
        Task<ResultModel> GetData(string token);
        Task<ResultModel> GetAppointmentStatistic(string token, DateTime? date);
    }
}
