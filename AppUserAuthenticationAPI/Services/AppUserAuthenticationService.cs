using AppUserAuthenticationAPI.DTOs.AppUserDTOs;
using AppUserAuthenticationAPI.DTOs.ResultModel;
using AppUserAuthenticationAPI.Repositories;
using AppUserAuthenticationAPI.Repository.OtpRepository;
using AppUserAuthenticationAPI.Utilities;
using AutoMapper;
using DataAccess.Models;
using DataAccess.Repositories;

namespace AppUserAuthenticationAPI.Services
{
    public class AppUserAuthenticationService : IAppUserAuthenticationService
    {

        private readonly IAppUserAuthenticationRepository _appUserAuthenticationRepository;
        private readonly IOtpRepository _otpRepository;

        public AppUserAuthenticationService(IAppUserAuthenticationRepository appUserAuthenticationRepository, IOtpRepository otpRepository)
        {
            _appUserAuthenticationRepository = appUserAuthenticationRepository;
            _otpRepository = otpRepository;

        }
        public async Task<ResultModel> Login(UserLoginReqModel userLoginReqModel)
        {
            ResultModel Result = new();
            try
            {
                var User = await _appUserAuthenticationRepository.GetUserByEmail(userLoginReqModel.Email);
                if (User == null)
                {
                    Result.IsSuccess = false;
                    Result.Code = 404;
                    Result.Message = "Email is not registered!";
                    return Result;
                }
                else if (User.Status != "ACTIVE")
                {
                    Result.IsSuccess = false;
                    Result.Code = 400;
                    Result.Message = "Please verify your account";
                    return Result;
                }
                else if (User.Role != "USER")
                {
                    Result.IsSuccess = false;
                    Result.Code = 401;
                    Result.Message = "Permission Denied";
                    return Result;
                }
                else
                {
                    var numberOfAppointment = await _appUserAuthenticationRepository.GetNumberOfAppointment(User.Id);
                    if(numberOfAppointment == 0)
                    {

                        User.TypeGroup = "First-Time Visitors";
                        await _appUserAuthenticationRepository.UpdateUserTypeGroup(User);
                    }
                    else if(numberOfAppointment > 0)
                    {
                        User.TypeGroup = "Normal Customer";
                        await _appUserAuthenticationRepository.UpdateUserTypeGroup(User);
                    }
                    if (numberOfAppointment > 3)
                    {
                        User.TypeGroup = "Loyalty Members";
                        await _appUserAuthenticationRepository.UpdateUserTypeGroup(User);
                    }
                    var Salt = User.Salt;
                    var PasswordStored = User.Password;
                    if (Salt != null && PasswordStored != null)
                    {
                        var Verify = Encoder.VerifyPasswordHashed(userLoginReqModel.Password, Salt, PasswordStored);
                        if (Verify)
                        {
                            if (User.Status == "RESETPASSWORD")
                            {
                                User.Status = "ACTIVE";
                                _ = await _appUserAuthenticationRepository.Update(User);
                            }
                            var config = new MapperConfiguration(cfg =>
                            {
                                cfg.CreateMap<User, UserResModel>();
                            });
                            IMapper mapper = config.CreateMapper();
                            UserResModel UserResModel = mapper.Map<User, UserResModel>(User);

                            UserLoginResModel LoginResData = new UserLoginResModel
                            {
                                User = UserResModel,
                                Token = Encoder.GenerateJWT(User)
                            };

                            Result.IsSuccess = true;
                            Result.Code = 200;
                            Result.Data = LoginResData;
                            User.LastLoggedIn = DateTime.Now;
                            _ = await _appUserAuthenticationRepository.Update(User);
                        }
                        else
                        {
                            Result.IsSuccess = false;
                            Result.Code = 400;
                            Result.Message = "Password is invalid";
                        }
                    }
                    else
                    {
                        Result.IsSuccess = false;
                        Result.Code = 400;
                        Result.Message = "User data is incomplete";
                    }
                }
            }
            catch (Exception e)
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.ResponseFailed = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            return Result;
        }

        public async Task<ResultModel> CreateAccount(UserReqModel RegisterForm)
        {
            ResultModel Result = new();
            try
            {
                if(RegisterForm.Password != RegisterForm.ConfirmPassword)
                {
                    Result.IsSuccess = false;
                    Result.Code = 400;
                    Result.Message = "Password and Confirm Password do not match!";
                    return Result;
                }
                var User = await _appUserAuthenticationRepository.GetUserByEmail(RegisterForm.Email);
                var UserPhoneNumber = await _appUserAuthenticationRepository.GetUserByPhoneNumber(RegisterForm.PhoneNumber);
                if (User != null)
                {
                    Result.IsSuccess = false;
                    Result.Code = 400;
                    Result.Message = "Email is already registered!";
                }
                else if (UserPhoneNumber != null)
                {
                    Result.IsSuccess = false;
                    Result.Code = 400;
                    Result.Message = "Phone number is already registered!";
                }
                else
                {
                    string OTP = GenerateOTP();
                    DateTime expirationTime = DateTime.Now.AddMinutes(10);
                    var config = new MapperConfiguration(cfg =>
                    {
                        cfg.CreateMap<UserReqModel, User>().ForMember(dest => dest.Password, opt => opt.Ignore());
                    });
                    IMapper mapper = config.CreateMapper();
                    User NewUser = mapper.Map<UserReqModel, User>(RegisterForm);
                    if (RegisterForm.Password == null)
                    {
                        RegisterForm.Password = Encoder.GenerateRandomPassword();
                    }
                    string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateEmail", "CreateAccount.html");

                    string Html = File.ReadAllText(FilePath);
                    Html = Html.Replace("{{Email}}", RegisterForm.Email);
                    Html = Html.Replace("{{OTP}}", $"{OTP}");

                    bool emailSent = await Email.SendEmail(RegisterForm.Email, "Email Verification", Html);

                    if (emailSent)
                    {
                        NewUser.Id = Guid.NewGuid();
                        NewUser.Status = "INACTIVE";
                        NewUser.CreatedAt = DateTime.Now;
                        NewUser.Role = "USER";
                        NewUser.TypeGroup = "First-Time Visitors";
                        var HashedPasswordModel = Encoder.CreateHashPassword(RegisterForm.Password);
                        NewUser.Password = HashedPasswordModel.HashedPassword;
                        NewUser.Salt = HashedPasswordModel.Salt;

                        _ = await _appUserAuthenticationRepository.Insert(NewUser);

                        OtpVerify otpVerify = new OtpVerify
                        {
                            Id = Guid.NewGuid(),
                            CreatedAt = DateTime.Now,
                            OtpCode = OTP,
                            ExpiredAt = expirationTime,
                            IsUsed = 0,
                            UserId = NewUser.Id,
                        };
                        _ = await _otpRepository.Insert(otpVerify);


                        Result.IsSuccess = true;
                        Result.Code = 200;
                        Result.Message = "Verification email sent successfully!";
                    }
                    else
                    {
                        // Handle email sending failure
                        Result.IsSuccess = false;
                        Result.Code = 500;
                        Result.Message = "Failed to send verification email. Please try again later.";
                    }
                }
            }
            catch (Exception e)
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.ResponseFailed = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            return Result;
        }

        private string GenerateOTP()
        {
            Random rnd = new Random();
            int otp = rnd.Next(100000, 999999);
            return otp.ToString();
        }

        public Task<object?> GetUserProfile(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
