using AppAppointmentManagementAPI.DTOs.ReviewModel;
using ProfileManagementAppAPI.DTOs.AppointmentDTOs;
using ProfileManagementAppAPI.DTOs.ResultModel;

namespace ProfileManagementAppAPI.Services
{
    public interface IAppointmentService 
    {
        Task<ResultModel> AddReview(string token, ReviewModel reviewModel);
        Task<ResultModel> GetCategory( string token);
        Task<ResultModel> GetListReview(string token);
        Task<ResultModel> UpdateReview(string token, ReviewUpdateModel reviewUpdateModel);
        Task<ResultModel> GetClinicInformation(string token);
        ///Note: Phòng ngừa khi get category call api không được.
      //  Task<ResultModel> GetServiceByCategoryID(string token, Guid categoryID);

      Task<ResultModel> GetPetListByUserId(string token);

      //BOOK APPOINTMENT
      Task<ResultModel> AddToCart(string token, AddToCartDTO addToCartDTO);
      Task<ResultModel> GetCartByUserId(string token);
      Task<ResultModel> BookAppointment(string token, UpdateCartDTO updateCartDTO);
      Task<ResultModel> RemoveServiceFromCart(string token, Guid serviceId);
      Task<ResultModel> GetListPromotion(string token);
      Task<ResultModel> GetPromotionByID(string token, Guid promotionId);
      Task<ResultModel> GetBookingHistory(string token);
      Task<ResultModel> GetListPromotionByAppointmentId(string token, Guid appointmentId);
      Task<ResultModel> CancelAppointment(string token, Guid appointmentId);
      Task<ResultModel> UpdateAppointment(string token, UpdateAppointmentDTO updateDTO);

      //BOOKING DETAIL
      Task<ResultModel> GetAppointmentDetail(string token, Guid appointmentId);
    }
}
