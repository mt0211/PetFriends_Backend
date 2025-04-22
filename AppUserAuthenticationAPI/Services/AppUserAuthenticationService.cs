using AppUserAuthenticationAPI.DTOs.AppUserDTOs;
using AppUserAuthenticationAPI.DTOs.ResultModel;
using AppUserAuthenticationAPI.Helpers;
using AppUserAuthenticationAPI.Repositories;
using AppUserAuthenticationAPI.Repository.OtpRepository;
using AppUserAuthenticationAPI.Utilities;
using AutoMapper;
using DataAccess.Models;
using DataAccess.Repositories;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace AppUserAuthenticationAPI.Services
{
    public class AppUserAuthenticationService : IAppUserAuthenticationService
    {

        private readonly IAppUserAuthenticationRepository _appUserAuthenticationRepository;
        private readonly IOtpRepository _otpRepository;
        private readonly IMessageBus _messageBus;
         private readonly GoogleOAuthOptions _googleOAuthOptions;
    private readonly ILogger<AppUserAuthenticationService> _logger;
        public AppUserAuthenticationService(IAppUserAuthenticationRepository appUserAuthenticationRepository, IOtpRepository otpRepository, IOptions<GoogleOAuthOptions> googleOAuthOptions, ILogger<AppUserAuthenticationService> logger, IMessageBus messageBus)
        {
            _appUserAuthenticationRepository = appUserAuthenticationRepository;
            _otpRepository = otpRepository;
            _googleOAuthOptions = googleOAuthOptions.Value;
            _logger = logger;
            _messageBus = messageBus;
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
                else if (User.Status != "ACTIVE" && User.FullName == null)
                {
                    Result.IsSuccess = false;
                    Result.Code = 400;
                    Result.Message = "Please verify your account";
                    return Result;
                }
                else if (User.Status != "ACTIVE" && User.FullName != null)
                {
                    Result.IsSuccess = false;
                    Result.Code = 400;
                    Result.Message = "Your account is banned. Please contect with clinic to unban your account";
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
                        NewUser.PhoneNumber = RegisterForm.PhoneNumber;
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

        
        public async Task<ResultModel> LoginWithGoogle(string googleToken)
        {
            ResultModel result = new();
            try
            {
                // Xác thực token Google
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = _googleOAuthOptions.AllowedClientIds.ToList()
                };
                
                 var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken, settings);
                
                // Kiểm tra xem người dùng đã tồn tại trong hệ thống chưa
                var user = await _appUserAuthenticationRepository.GetUserByEmail(payload.Email);
                
                if (user == null)
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "Email is not registered in the system. Please register an account before logging in with Google.";
                    return result;
                    
                }
            
                if (user.Status != "ACTIVE")
                {
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "Can't login with Google because your account is not active";
                    return result;
                }
                user.LastLoggedIn = DateTime.Now;
                await _appUserAuthenticationRepository.Update(user);
                // Tạo JWT token
                var token = Encoder.GenerateJWT(user);
                
                // Tạo response model
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<User, UserResModel>();
                });
                IMapper mapper = config.CreateMapper();
                UserResModel userResModel = mapper.Map<User, UserResModel>(user);

                UserLoginResModel loginResData = new UserLoginResModel
                {
                    User = userResModel,
                    Token = token
                };
                
                result.IsSuccess = true;
                result.Code = 200;
                result.Message = "Login successful";
                result.Data = loginResData;
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning($"Invalid Google token: {ex.Message}");
                result.IsSuccess = false;
                result.Code = 401;
                result.Message = "Invalid Google token";
                result.ResponseFailed = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google login");
                result.IsSuccess = false;
                result.Code = 500;
                result.Message = "An unexpected error occurred";
                result.ResponseFailed = ex.InnerException != null 
                    ? $"{ex.InnerException.Message}\n{ex.StackTrace}"
                    : $"{ex.Message}\n{ex.StackTrace}";
            }

        return result;
        }
        public async Task<ResultModel> SignUpWithGoogle(string googleToken)
        {
            ResultModel result = new();
            try
            {
                // Xác thực token Google
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = _googleOAuthOptions.AllowedClientIds.ToList()
                };
                
                var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken, settings);
                
                // Kiểm tra xem email đã tồn tại chưa
                var existingUser = await _appUserAuthenticationRepository.GetUserByEmail(payload.Email);
                
                if (existingUser != null)
                {
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "This email is already registered in the system. Please login with your account";
                    return result;
                }

                // Tạo tài khoản mới
                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = payload.Email,
                    FullName = payload.Name,
                    AvatarUrl = payload.Picture,
                    Status = "ACTIVE",
                    CreatedAt = DateTime.Now.AddHours(7),
                    Role = "USER",
                    LastLoggedIn = DateTime.Now,
                    TypeGroup = "First-Time Visitors"
                    
                };
                
                await _appUserAuthenticationRepository.Insert(newUser);
                _logger.LogInformation($"Created new user via Google signup: {payload.Email}");

                // Tạo JWT token
                var token = Encoder.GenerateJWT(newUser);
                
                // Tạo response model
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<User, UserResModel>();
                });
                IMapper mapper = config.CreateMapper();
                UserResModel userResModel = mapper.Map<User, UserResModel>(newUser);

                UserLoginResModel signUpResData = new UserLoginResModel
                {
                    User = userResModel,
                    Token = token
                };
                
                result.IsSuccess = true;
                result.Code = 201; // Created
                result.Message = "Sign up successfully";
                result.Data = signUpResData;
                _messageBus.PublicUserActivity
                    (
                        "USER_CREATED",
                        newUser.Id
                    );
                
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning($"Invalid Google token: {ex.Message}");
                result.IsSuccess = false;
                result.Code = 401;
                result.Message = "Invalid Google token";
                result.ResponseFailed = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google signup");
                result.IsSuccess = false;
                result.Code = 500;
                result.Message = "An error occurred during signup";
                result.ResponseFailed = ex.InnerException != null 
                    ? $"{ex.InnerException.Message}\n{ex.StackTrace}"
                    : $"{ex.Message}\n{ex.StackTrace}";
            }

            return result;
        }
        public async Task<ResultModel> UpdateUserFullName(UpdateUserFullName userUpdateFullNameandPhoneNumberModel)
        {
            ResultModel Result = new();
            try
            {
                var user = await _appUserAuthenticationRepository.Get(userUpdateFullNameandPhoneNumberModel.Id);
                
                if (user == null)
                {
                    Result.IsSuccess = false;
                    Result.Code = 400;
                    Result.Message = "Not found";
                    return Result;
                }else if (user.TypeGroup != "First-Time Visitors")
                {
                    Result.IsSuccess = false;
                    Result.Code = 400;
                    Result.Message = "This method is only available for first-time visitors";
                    return Result;
                }
                user.FullName = userUpdateFullNameandPhoneNumberModel.FullName;
                _ = await _appUserAuthenticationRepository.Update(user);
                var userDto = new 
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Status = user.Status,
                    TypeGroup = user.TypeGroup,
                    CreatedAt = user.CreatedAt,
                    LastLoggedIn = user.LastLoggedIn
                };
                Result.IsSuccess = true;
                Result.Data = userDto;
                Result.Code = 200;
                Result.Message = "Updated full name successfully";
                   var chatMessage = new ChatMessage
                        {
                            Id              = Guid.NewGuid(),
                            SenderId        = Guid.Parse("4EB49A8F-889A-4A62-8646-D9624EA0F372"),
                            ReceiverId      = user.Id,
                            Content         = "Hi, how can I assist you today?",
                            SentTime        = DateTime.UtcNow.AddHours(7),
                            IsRead          = false,
                            MessageType     = "TEXT",
                            MediaUrl        = null,
                            CreatedAt       = DateTime.UtcNow.AddHours(7),
                            IsDeleteForSender   = false,
                            IsDeleteForReceiver = false
                        };
                        await _appUserAuthenticationRepository.CreateChatMessage(chatMessage);
            }
            catch (Exception ex)
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.Message = ex.Message;

            }
            return Result;
        }
    }
}
