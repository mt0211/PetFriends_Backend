using ClinicServiceManagement.DTOs.ResultModel;

namespace ClinicServiceManagement.Services
{
    public interface IClinicServiceService
    {
        Task<ResultModel> GetAllService(string token);
    }
}
