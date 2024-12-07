using AutoMapper;
using UserAuthenticationAPI.DTOs.ResultModel;
using UserAuthenticationAPI.DTOs.UserDTOs;
using UserAuthenticationAPI.Utilities;
using DataAccess.Models;
using UserAuthenticationAPI.Repository.UserRepository;
using UserAuthenticationAPI.Repository.OtpRepository;
namespace UserAuthenticationAPI.Services;
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpRepository _otpRepository;
    public UserService(IUserRepository userRepository, IOtpRepository otpRepository)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
    }
    public async Task<ResultModel> Login(UserLoginReqModel LoginForm)
    {
        ResultModel Result = new();
        try
        {
            var User = await _userRepository.GetUserByEmail(LoginForm.Email);
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
            else if (User.Role != "PARTNER")
            {
                Result.IsSuccess = false;
                Result.Code = 401;
                Result.Message = "Permission Denied";
                return Result;
            }else
            {
                var Salt = User.Salt;
                var PasswordStored = User.Password;
                if (Salt != null && PasswordStored != null)
                {
                    var Verify = Encoder.VerifyPasswordHashed(LoginForm.Password, Salt, PasswordStored);
                    if (Verify)
                    {
                        if (User.Status == "RESETPASSWORD")
                        {
                            User.Status = "ACTIVE";
                            _ = await _userRepository.Update(User);
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
                        _ = await _userRepository.Update(User);
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
           
            var User = await _userRepository.GetUserByEmail(RegisterForm.Email);
            var UserPhoneNumber = await _userRepository.GetUserByPhoneNumber(RegisterForm.PhoneNumber);
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
                    NewUser.Role = "PARTNER";
                    var HashedPasswordModel = Encoder.CreateHashPassword(RegisterForm.Password);
                    NewUser.Password = HashedPasswordModel.HashedPassword;
                    NewUser.Salt = HashedPasswordModel.Salt;

                    _ = await _userRepository.Insert(NewUser);

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

    public async Task<ResultModel> GetUserProfile(Guid userId)
    {
        ResultModel Result = new();
        try
        {
            var user = await _userRepository.Get(userId);

            if (user == null)
            {
                Result.IsSuccess = false;
                Result.Code = 404;
                Result.Message = "Not found";
                return Result;
            }

            var userProfile = new
            {
                id = user.Id,
                user.FullName,
                user.Email,
                user.Address,
                user.Dob,
                user.Gender,
                phoneNumber = user.PhoneNumber,
                user.Role,
                user.AvatarUrl,

            };

            Result.IsSuccess = true;
            Result.Code = 200;
            Result.Data = userProfile;
        }
        catch (Exception e)
        {
            Result.IsSuccess = false;
            Result.Code = 400;
            Result.ResponseFailed = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
        }
        return Result;
    }

    public async Task<ResultModel> UpdateUserProfile(UserUpdateModel updateModel)
    {
        ResultModel Result = new();
        try
        {
            var user = await _userRepository.Get(updateModel.Id);

            if (user == null)
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.Message = "Not found";
                return Result;
            }
           // user.Email = updateModel.Email;
            user.PhoneNumber = updateModel.PhoneNumber;
            user.Address = updateModel.Address;
            user.Gender = updateModel.Gender;
            user.FullName = updateModel.FullName;
            user.Dob = updateModel.Dob;
            user.AvatarUrl = updateModel.AvatarUrl;

            _ = await _userRepository.Update(user);
            Result.IsSuccess = true;
            Result.Data = user;
            Result.Code = 200;
            Result.Message = "Profile updated successfully";
        }
        catch (Exception ex)
        {
            Result.IsSuccess = false;
            Result.Code = 400;
            Result.Message = ex.Message;

        }
        return Result;
    }

    public async Task<ResultModel> ChangePassword(Guid userId, ChangePasswordReqModel changePasswordModel)
    {
        ResultModel result = new ResultModel();
        try
        {
            var user = await _userRepository.Get(userId);

            if (user == null)
            {
                result.IsSuccess = false;
                result.Code = 404; // Not Found
                result.Message = "User not found";
                return result;
            }

            // Verify the old password
            var oldPasswordHash = user.Password;
            var oldPasswordSalt = user.Salt;
            var isOldPasswordValid = Encoder.VerifyPasswordHashed(changePasswordModel.OldPassword, oldPasswordSalt, oldPasswordHash);

            if (!isOldPasswordValid)
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "Old password is incorrect";
                return result;
            }

            // Generate new password hash and salt
            var newPasswordHashModel = Encoder.CreateHashPassword(changePasswordModel.NewPassword);
            user.Password = newPasswordHashModel.HashedPassword;
            user.Salt = newPasswordHashModel.Salt;

            // Update the user in the database
            _ = await _userRepository.Update(user);

            result.IsSuccess = true;
            result.Code = 200;
            result.Message = "Password updated successfully";
        }
        catch (Exception e)
        {
            result.IsSuccess = false;
            result.Code = 500; // Internal Server Error
            result.ResponseFailed = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
        }
        return result;
    }

    public async Task<ResultModel> ResetPassword(UserResetPasswordReqModel ResetPasswordReqModel)
    {
        ResultModel Result = new();
        try
        {
            var User = await _userRepository.GetUserByEmail(ResetPasswordReqModel.Email);
            if (User == null)
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.Message = "The User cannot validate to reset password";
                return Result;
            }
            if (User.Status != "RESETPASSWORD")
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.Message = "The request is denied!";
                return Result;
            }
            var HashedPasswordModel = Encoder.CreateHashPassword(ResetPasswordReqModel.Password);
            User.Password = HashedPasswordModel.HashedPassword;
            User.Salt = HashedPasswordModel.Salt;
            User.Status = "ACTIVE";
            _ = await _userRepository.Update(User);
            Result.IsSuccess = true;
            Result.Code = 200;
            Result.Message = "Reset password successfully!";
        }
        catch (Exception e)
        {
            Result.IsSuccess = false;
            Result.Code = 400;
            Result.ResponseFailed = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
        }
        return Result;
    }
}