using AppUserAuthenticationAPI.DTOs.AppUserDTOs;
using AppUserAuthenticationAPI.DTOs.ResultModel;
using AppUserAuthenticationAPI.Repositories;
using AppUserAuthenticationAPI.Utilities;
using AutoMapper;
using DataAccess.Models;
using DataAccess.Repositories;

namespace AppUserAuthenticationAPI.Services
{
    public class AppUserAuthenticationService : IAppUserAuthenticationService
    {

        private readonly IAppUserAuthenticationRepository _appUserAuthenticationRepository;
        public AppUserAuthenticationService(IAppUserAuthenticationRepository appUserAuthenticationRepository)
        {
            _appUserAuthenticationRepository = appUserAuthenticationRepository;
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
    }
}
