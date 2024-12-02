using UserAuthenticationAPI.DTOs.ResultModel;
using UserAuthenticationAPI.DTOs.UserDTOs;

namespace UserAuthenticationAPI.Services;

public interface IUserService
{
  public Task<ResultModel> Login(UserLoginReqModel LoginForm);
  public Task<ResultModel> CreateAccount(UserReqModel RegisterForm);
  Task<ResultModel> GetUserProfile(Guid userId);
  Task<ResultModel> UpdateUserProfile(UserUpdateModel updateModel);
  Task<ResultModel> ChangePassword(Guid userId, ChangePasswordReqModel changePasswordModel);
  public Task<ResultModel> ResetPassword(UserResetPasswordReqModel ResetPasswordReqModel);
}