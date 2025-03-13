using AppUserAuthenticationAPI.DTOs.AppUserDTOs;
using AppUserAuthenticationAPI.DTOs.ResultModel;

namespace AppUserAuthenticationAPI.Services
{
    public interface IAppUserAuthenticationService 
    {
        Task<ResultModel> CreateAccount(UserReqModel form);
        Task<object?> GetUserProfile(Guid userId);
        Task<ResultModel> Login(UserLoginReqModel userLoginReqModel);
        Task<ResultModel> LoginWithGoogle(string googleToken);
        Task<ResultModel> SignUpWithGoogle(string googleToken);
    }
}
