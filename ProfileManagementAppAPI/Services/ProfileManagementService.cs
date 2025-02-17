using DataAccess.Models;
using ProfileManagementAppAPI.DTOs.ResultModel;
using ProfileManagementAppAPI.DTOs.;

using ProfileManagementAppAPI.Repositories;
using ProfileManagementAppAPI.Utilities;
using ProfileManagementAppAPI.DTOs.ClinicProfileModel;

namespace ProfileManagementAppAPI.Services
{
    public class ProfileManagementService : IProfileManagementService
    {
        private readonly IProfileManagementRepository _profileManagementRepository;
        public ProfileManagementService(IProfileManagementRepository profileManagementRepository)
        {
            _profileManagementRepository = profileManagementRepository;
        }

        public async Task<ResultModel> GetUserProfile(Guid userId)
        {
            ResultModel Result = new();
            try
            {
                var user = await _profileManagementRepository.Get(userId);

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
                var user = await _profileManagementRepository.Get(updateModel.Id);

                if (user == null)
                {
                    Result.IsSuccess = false;
                    Result.Code = 400;
                    Result.Message = "Not found";
                    return Result;
                }
                user.Email = updateModel.Email;
                user.PhoneNumber = updateModel.PhoneNumber;
                user.Address = updateModel.Address;
                user.Gender = updateModel.Gender;
                user.FullName = updateModel.FullName;
                user.Dob = updateModel.Dob;
                user.AvatarUrl = updateModel.AvatarUrl;

                _ = await _profileManagementRepository.Update(user);
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
    }
}
