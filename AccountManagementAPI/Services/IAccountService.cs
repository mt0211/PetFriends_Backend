using AccountManagementAPI.DTOs.ResultModel;
using AccountManagementAPI.DTOs.UserDTOs;

namespace AccountManagementAPI.Services
{
    public interface IAccountService
    {
        Task<ResultModel> GetAllAccount(string token, int page);
        Task<ResultModel> UpdateAccountStatus(string token, UserUpdateStatusModel userUpdateStatusModel);
        Task<ResultModel> AddNewAccount(string token, UserAddModel userAddModel);
        Task<ResultModel> GetAccountDetail(string token, Guid AccountID);
        Task<ResultModel> UpdateAccount(string token, UserUpdateModel userUpdateModel);
    }
}
