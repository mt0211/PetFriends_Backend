using ProfileManagementAppAPI.DTOs.ClinicProfileModel;
using ProfileManagementAppAPI.DTOs.ResultModel;

namespace ProfileManagementAppAPI.Services
{
    public interface IAppointmentService 
    {
        Task<ResultModel> GetCategory( string token);

    }
}
