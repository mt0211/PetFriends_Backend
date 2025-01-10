using ClinicServiceManagement.DTOs.ResultModel;
using ClinicServiceManagement.DTOs.ServiceDTOs;

namespace ClinicServiceManagementAPI.Services.ClinicServiceServices
{
    public interface IClinicServiceService
    {
        Task<ResultModel> GetAllService(string token);
        Task<ResultModel> AddNewService(string token, ServiceAddDTO serviceAddDTO);
        Task<ResultModel> GetAllCategory(string token);
        Task<ResultModel> GetServiceDetail(string token, Guid ServiceID);
        Task<ResultModel> UpdateServiceStatus(string token, Guid serviceId);
        Task<ResultModel> UpdateService(string token, ServiceUpdateDTO serviceUpdateDTO);
    }
}
