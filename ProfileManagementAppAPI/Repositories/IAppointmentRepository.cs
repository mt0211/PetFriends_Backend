

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

       Task<UserCart> CheckUserCart(Guid userId);
       Task AddNewCart(UserCart userCart);
       Task AddNewCartItem(UserCartItem userCartItem);
       Task<UserCartItem> CheckUserCartItemByServiceId(Guid serviceId);
       Task<UserCart> GetCartByUserId(Guid userId);

       Task UpdateCart(UserCart userCart);
       Task AddAppointment(Appointment appointment);
         Task AddAppointmentClinicService(AppointmentClinicService appointmentService);
         Task<List<UserCartItem>> GetCartItemsByCartId(Guid cartId);
         Task<UserCartItem> GetCartItemByServiceId(Guid serviceId);
        Task RemoveCartItem(UserCartItem cartItem);
        Task RemoveCart(UserCart cart);
        Task<List<Promotion>> GetPromotionTypeAllMember();
        Task<List<Promotion>> GetPromotionTypeNewMember();
        Task<List<Promotion>> GetPromotionTypeLoyaltyMember();
        Task<User> GetUserByUserId(Guid userId);
    }
}
