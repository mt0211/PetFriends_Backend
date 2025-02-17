using ProfileManagementAppAPI.DTOs.ClinicProfileModel;
using ProfileManagementAppAPI.DTOs.ResultModel;

namespace ProfileManagementAppAPI.Services
{
    public interface IProfileManagementService
    {
        Task<ResultModel> GetUserProfile(Guid userId);
        Task<ResultModel> UpdateUserProfile(UserUpdateModel updateModel);
    }
}
