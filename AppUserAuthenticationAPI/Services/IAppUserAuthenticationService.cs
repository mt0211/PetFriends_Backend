using AppUserAuthenticationAPI.DTOs.AppUserDTOs;
using AppUserAuthenticationAPI.DTOs.ResultModel;

namespace AppUserAuthenticationAPI.Services
{
    public interface IAppUserAuthenticationService 
    {
        Task<ResultModel> Login(UserLoginReqModel userLoginReqModel);
    }
}
