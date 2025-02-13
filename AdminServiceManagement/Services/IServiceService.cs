using AdminServiceManagement.DTOs.ResultModel.CustomerReviews.DTOs.ResultModel;
using AdminServiceManagement.DTOs.ServiceDTOs;

namespace AdminServiceManagement.Services
{
    public interface IServiceService
    {
        Task<ResultModel> GetAllService(string token, int page);
        Task<ResultModel> UpdateServiceStatus(string token, ServiceUpdateRequestModel ServiceUpdateDTO);
    }
}
