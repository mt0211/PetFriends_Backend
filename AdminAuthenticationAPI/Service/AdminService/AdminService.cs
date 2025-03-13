using AdminAuthenticationAPI.DTOs.AdminDTOs;
using AdminAuthenticationAPI.DTOs.ResultModelAdmin;
using AdminAuthenticationAPI.Helpers;
using AdminAuthenticationAPI.Repository.AdminRepository;
//using AdminAuthenticationAPI.Repository.VerifyAdmminRepository;
using AdminAuthenticationAPI.Utilities;
using AutoMapper;
using DataAccess.Models;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using MySqlX.XDevAPI.Common;

namespace AdminAuthenticationAPI.Service.AdminService
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;
         private readonly GoogleOAuthOptions _googleOAuthOptions;
    private readonly ILogger<AdminService> _logger;

        public AdminService(IAdminRepository adminRepository, IOptions<GoogleOAuthOptions> googleOAuthOptions, ILogger<AdminService> logger)
        {
            _adminRepository = adminRepository;
             _googleOAuthOptions = googleOAuthOptions.Value;
            _logger = logger;
        }

        public async Task<ResultModelAdmin> LoginAdmin(AdminReqModel.AdminLoginReqModel adminLoginReqModel)
        {
            ResultModelAdmin Result = new();
            try
            {
                var User = await _adminRepository.GetAdminByEmail(adminLoginReqModel.Email);
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
                else if (User.Role != "ADMIN")
                {
                    Result.IsSuccess = false;
                    Result.Code = 401;
                    Result.Message = "Permission Denied";
                    return Result;
                }
                else
                {
                    var Salt = User.Salt;
                    var PasswordStored = User.Password;
                    if (Salt != null && PasswordStored != null)
                    {
                        var Verify = Encoder.VerifyPasswordHashed(adminLoginReqModel.Password, Salt, PasswordStored);
                        if (Verify)
                        {
                            if (User.Status == "RESETPASSWORD")
                            {
                                User.Status = "ACTIVE";
                                _ = await _adminRepository.Update(User);
                            }
                            var config = new MapperConfiguration(cfg =>
                            {
                                cfg.CreateMap<User, AdminRepModel>();
                            });
                            IMapper mapper = config.CreateMapper();
                            AdminRepModel AdminRepModel = mapper.Map<User, AdminRepModel>(User);

                            AdminLoginResModel LoginResData = new AdminLoginResModel
                            {
                                User = AdminRepModel,
                                Token = Encoder.GenerateJWT(User)
                            };

                            Result.IsSuccess = true;
                            Result.Code = 200;
                            Result.Data = LoginResData;
                            User.LastLoggedIn = DateTime.Now;
                            _ = await _adminRepository.Update(User);
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
        public async Task<ResultModelAdmin> LoginWithGoogle(string googleToken)
        {
            ResultModelAdmin result = new();
            try
            {
                // Xác thực token Google
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string> { _googleOAuthOptions.ClientId }
                };
                
                var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken, settings);
                
                // Kiểm tra xem người dùng đã tồn tại trong hệ thống chưa
                var user = await _adminRepository.GetAdminByEmail(payload.Email);
                
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
                await _adminRepository.Update(user);
                // Tạo JWT token
                var token = Encoder.GenerateJWT(user);
                
                // Tạo response model
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<User, AdminRepModel>();
                });
                IMapper mapper = config.CreateMapper();
                AdminRepModel userResModel = mapper.Map<User, AdminRepModel>(user);

                AdminLoginResModel loginResData = new AdminLoginResModel
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
    }
}
