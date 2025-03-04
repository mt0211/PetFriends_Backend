

using AppAppointmentManagementAPI.DTOs.ReviewModel;
using DataAccess.Models;
using DataAccess.Repositories;

namespace ProfileManagementAppAPI.Repositories
{
    public interface IAppointmentRepository 
    {
        Task AddReview(Feedback reviewEntity);
        Task<IEnumerable<Category>> GetCategory();
        Task<IEnumerable<dynamic>> GetReview();
        Task<Feedback> GetReviewById(Guid reviewId);
        Task UpdateReview(Feedback reviewEntity);
        Task<dynamic> GetClinicInformation();

        ///Note: Phòng ngừa khi get category call api không được.
       // Task<dynamic> GetServiceByCategoryID(Guid categoryID);

       Task<IEnumerable<Pet>> GetPetListByUserId(Guid userId);
    }
}
