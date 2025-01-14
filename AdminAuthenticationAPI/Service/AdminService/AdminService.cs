using AdminAuthenticationAPI.DTOs.AdminDTOs;
using AdminAuthenticationAPI.DTOs.ResultModelAdmin;
using AdminAuthenticationAPI.Repository.AdminRepository;
//using AdminAuthenticationAPI.Repository.VerifyAdmminRepository;
using AdminAuthenticationAPI.Utilities;
using AutoMapper;
using DataAccess.Models;
using MySqlX.XDevAPI.Common;

namespace AdminAuthenticationAPI.Service.AdminService
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;

        public AdminService(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
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
    }
}
