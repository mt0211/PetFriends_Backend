

using AppAppointmentManagementAPI.DTOs.ReviewModel;
using DataAccess.Models;
using DataAccess.Repositories;

namespace ProfileManagementAppAPI.Repositories
{
    public interface IAppointmentRepository 
    {
        Task AddReview(Feedback reviewEntity);
        Task<IEnumerable<Category>> GetCategory();
        Task<IEnumerable<Feedback>> GetReview();
        Task<Feedback> GetReviewById(Guid reviewId);
        Task UpdateReview(Feedback reviewEntity);
        Task<dynamic> GetClinicInformation();
        Task<dynamic> GetAppointmentByUserID(Guid userID);
    }
}
