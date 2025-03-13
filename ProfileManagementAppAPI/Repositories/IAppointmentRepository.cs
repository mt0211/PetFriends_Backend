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
        Task<Feedback> GetReviewByAppointmentId(Guid appointmentId);
        Task UpdateReview(Feedback reviewEntity);
        Task<dynamic> GetClinicInformation();

        ///Note: Phòng ngừa khi get category call api không được.
       // Task<dynamic> GetServiceByCategoryID(Guid categoryID);

       Task<IEnumerable<Pet>> GetPetListByUserId(Guid userId);

       Task<UserCart> CheckUserCart(Guid userId);
       Task AddNewCart(UserCart userCart);
       Task AddNewCartItem(UserCartItem userCartItem);
       Task<UserCartItem> CheckUserCartItemByServiceId(Guid serviceId, Guid userId);
       Task<UserCart> GetCartByUserId(Guid userId);

       Task UpdateCart(UserCart userCart);
       Task AddAppointment(Appointment appointment);
         Task AddAppointmentClinicService(AppointmentClinicService appointmentService);
         Task<List<UserCartItem>> GetCartItemsByCartId(Guid cartId);
        Task RemoveCartItem(UserCartItem cartItem);
        Task RemoveCart(UserCart cart);
        Task<List<Promotion>> GetPromotionTypeAllMember();
        Task<List<Promotion>> GetPromotionTypeNewMember();
        Task<List<Promotion>> GetPromotionTypeLoyaltyMember();
        Task<User> GetUserByUserId(Guid userId);
        Task<Promotion> GetPromotionById(Guid promotionId);
        Task AddAppointmentPromotion(AppointmentPromotion appointmentPromotion);
        Task<List<Appointment>> GetBookingHistory(Guid userId);
        Task<List<AppointmentPromotion>> GetListPromotionByAppointmentId(Guid appointmentId);
        Task CancelAppointment(Guid appointmentId);
        Task<Appointment> GetAppointmentById(Guid appointmentId);

        //Edit Pending Appointment  
        Task<List<AppointmentClinicService>> GetAppointmentServices(Guid appointmentId);
        Task<List<AppointmentPromotion>> GetAppointmentPromotions(Guid appointmentId);
        Task RemoveAppointmentService(AppointmentClinicService service);
         Task RemoveAppointmentPromotion(AppointmentPromotion promotion);
         Task UpdateAppointment(Appointment appointment);
         Task<Appointment> GetAppointmentDetailById(Guid appointmentId);
         Task<bool> CheckReview(Guid appointmentId);
         Task<int> CountService(Guid cartId);
    }
}
