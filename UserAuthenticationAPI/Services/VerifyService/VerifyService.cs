using DataAccess.Models;
using UserAuthenticationAPI.DTOs.ResultModel;
using UserAuthenticationAPI.Repository.OtpRepository;
using UserAuthenticationAPI.Repository.UserRepository;
using UserAuthenticationAPI.Utilities;

namespace UserAuthenticationAPI.Services;
public class VerifyService : IVerifyService
{
    private readonly IOtpRepository _otpRepository;
    private readonly IUserRepository _userRepository;
    public VerifyService(IOtpRepository otpRepository, IUserRepository userRepository)
    {
        _otpRepository = otpRepository;
        _userRepository = userRepository;
    }
    private string CreateOTPCode()
    {
        Random rnd = new();
        return rnd.Next(100000, 999999).ToString();
    }

    public async Task<ResultModel> SendOTPEmailRequest(string email)
    {
        ResultModel Result = new ResultModel();
        try
        {
            var User = await _userRepository.GetUserByEmail(email);
            if (User == null)
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.Message = "The User with this email is invalid";
                return Result;
            }
            var GetOTP = await _otpRepository.GetOTPByUserId(User.Id);
            if (GetOTP != null)
            {
                if ((DateTime.Now - GetOTP.CreatedAt).TotalMinutes < 2)
                {
                    Result.IsSuccess = false;
                    Result.Code = 400;
                    Result.Message = "Can not send OTP right now! Please try again after 2 minutes";
                    return Result;
                }
            }

            string OTPCode = CreateOTPCode();
            string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateEmail", "ResetPassword.html");
            string Html = File.ReadAllText(FilePath);
            Html = Html.Replace("{{OTPCode}}", OTPCode);
            Html = Html.Replace("{{toEmail}}", email);
            bool check = await Email.SendEmail(email, "Reset Password", Html);
            if (!check)
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.Message = "Send email is failed!";
                return Result;
            }
            OtpVerify Otp = new()
            {
                Id = Guid.NewGuid(),
                UserId = User.Id,
                OtpCode = OTPCode,
                CreatedAt = DateTime.Now,
                ExpiredAt = DateTime.Now.AddMinutes(10),
                IsUsed = 0
            };
            _ = await _otpRepository.Insert(Otp);
            Result.IsSuccess = true;
            Result.Code = 200;
            Result.Message = "The OTP code has been sent to your email";
        }
        catch (Exception e)
        {
            Result.IsSuccess = false;
            Result.Code = 400;
            Result.ResponseFailed = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
        }
        return Result;
    }

    public async Task<ResultModel> VerifyEmail(string email, string otpCode)
    {
        var result = new ResultModel();

        try
        {
            var user = await _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "User not found.";
                return result;
            }

            var otp = await _otpRepository.GetOTPByUserId(user.Id);
            if (otp == null || otp.IsUsed == 1 || (DateTime.Now - otp.CreatedAt).TotalMinutes > 10)
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "OTP expired or already used.";
                return result;
            }

            if (otp.OtpCode != otpCode)
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "Incorrect OTP.";
                return result;
            }
            otp.IsUsed = 1;
            await _otpRepository.Update(otp);
            user.Status = "ACTIVE";
            await _userRepository.Update(user);
            result.IsSuccess = true;
            result.Message = "OTP verified successfully.";
            result.Code = 200;
        }
        catch (Exception e)
        {
            result.IsSuccess = false;
            result.Code = 500;
            result.Message = "An unexpected error occurred while verifying OTP.";
            result.ResponseFailed = e.Message;
        }

        return result;
    }

    public async Task<ResultModel> VerifyResetPassword(string email, string otpCode)
    {
        var result = new ResultModel();

        try
        {
            var user = await _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "User not found.";
                return result;
            }

            var otp = await _otpRepository.GetOTPByUserId(user.Id);
            if (otp == null || otp.IsUsed == 1 || (DateTime.Now - otp.CreatedAt).TotalMinutes > 10)
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "OTP expired or already used.";
                return result;
            }

            if (otp.OtpCode != otpCode)
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "Incorrect OTP.";
                return result;
            }
            otp.IsUsed = 1;
            await _otpRepository.Update(otp);
            user.Status = "RESETPASSWORD";
            await _userRepository.Update(user);
            result.IsSuccess = true;
            result.Message = "OTP verified successfully.";
            result.Code = 200;
        }
        catch (Exception e)
        {
            result.IsSuccess = false;
            result.Code = 500;
            result.Message = "An unexpected error occurred while verifying OTP.";
            result.ResponseFailed = e.Message;
        }

        return result;
    }
}