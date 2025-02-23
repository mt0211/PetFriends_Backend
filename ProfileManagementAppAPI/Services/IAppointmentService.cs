using AppAppointmentManagementAPI.DTOs.ReviewModel;
using ProfileManagementAppAPI.DTOs.ClinicProfileModel;
using ProfileManagementAppAPI.DTOs.ResultModel;

namespace ProfileManagementAppAPI.Services
{
    public interface IAppointmentService 
    {
        Task<ResultModel> AddReview(string token, ReviewModel reviewModel);
        Task<ResultModel> GetCategory( string token);
        Task<ResultModel> GetListReview(string token);
        Task<ResultModel> UpdateReview(string token, ReviewUpdateModel reviewUpdateModel);
    }
}
