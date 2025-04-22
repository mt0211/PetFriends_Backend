using AppUserAuthenticationAPI.DTOs.ResultModel;

namespace AppUserAuthenticationAPI.Services;
public interface IVerifyService
{
    public Task<ResultModel> SendOTPEmailRequest(string email);
    public Task<ResultModel> VerifyEmail(string email, string otpCode);
    public Task<ResultModel> VerifyResetPassword(string email, string otpCode);

}