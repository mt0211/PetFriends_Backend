using Microsoft.AspNetCore.Mvc;
using ProfileManagementAppAPI.Services;
using ProfileManagementAppAPI.DTOs;

using DataAccess.Models;
using ProfileManagementAppAPI.DTOs.ResultModel;
using AppAppointmentManagementAPI.DTOs.ReviewModel;
using ProfileManagementAppAPI.DTOs.AppointmentDTOs;
namespace ProfileManagementAppAPI.Controllers
{



    [ApiController]
    [Route("api/userappointment")]

    public class AppAppointmentManagementController : ControllerBase
    {
        private readonly IAppointmentService _profileManagementService;

        public AppAppointmentManagementController(IAppointmentService profileManagementService)
        {
            _profileManagementService = profileManagementService;
        }

        [HttpGet("view-category")]
        public async Task<IActionResult> GetCategory()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _profileManagementService.GetCategory(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
           
        }

        [HttpGet("review-list")]
        public async Task<IActionResult> GetListReview()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _profileManagementService.GetListReview(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPost("add-review")]
        public async Task<IActionResult> AddReview([FromBody] ReviewModel reviewModel)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _profileManagementService.AddReview(token, reviewModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("update-review")]
        public async Task<IActionResult> UpdateReview([FromBody] ReviewUpdateModel reviewUpdateModel)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _profileManagementService.UpdateReview(token, reviewUpdateModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("clinic-information")]
        public async Task<IActionResult> GetClinicInformation()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _profileManagementService.GetClinicInformation(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        ///Note: Phòng ngừa khi get category call api không được.
        // [HttpGet("service-by-category-id")]
        // public async Task<IActionResult> GetServiceByCategoryID([FromQuery] Guid categoryID)
        // {
        //     string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
        //     ResultModel result = await _profileManagementService.GetServiceByCategoryID(token, categoryID);
        //     return result.IsSuccess ? Ok(result) : BadRequest(result);
        // }
        [HttpGet("get-pet-list-by-user-id")]
        public async Task<IActionResult> GetPetListByUserId()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _profileManagementService.GetPetListByUserId(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPost("add-to-cart")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDTO addToCartDTO)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _profileManagementService.AddToCart(token, addToCartDTO);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("get-cart-by-user-id")]
        public async Task<IActionResult> GetCartByUserId()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _profileManagementService.GetCartByUserId(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("book-appointment")]
        public async Task<IActionResult> BookAppointment([FromBody] UpdateCartDTO updateCartDTO)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _profileManagementService.BookAppointment(token, updateCartDTO);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpDelete("remove-service-from-cart")]
        public async Task<IActionResult> RemoveServiceFromCart([FromQuery] Guid serviceId)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _profileManagementService.RemoveServiceFromCart(token, serviceId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("get-list-promotion")]
        public async Task<IActionResult> GetListPromotion()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _profileManagementService.GetListPromotion(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("get-promotion-by-id")]
        public async Task<IActionResult> GetPromotionByID([FromQuery] Guid promotionId)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _profileManagementService.GetPromotionByID(token, promotionId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("get-booking-history")]
        public async Task<IActionResult> GetBookingHistory()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _profileManagementService.GetBookingHistory(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("get-list-promotion-by-appointment-id")]
        public async Task<IActionResult> GetListPromotionByAppointmentId([FromQuery] Guid appointmentId)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _profileManagementService.GetListPromotionByAppointmentId(token, appointmentId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("cancel-appointment")]
        public async Task<IActionResult> CancelAppointment([FromQuery] Guid appointmentId)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _profileManagementService.CancelAppointment(token, appointmentId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("update-appointment")]
        public async Task<IActionResult> UpdateAppointment([FromBody] UpdateAppointmentDTO updateDTO)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _profileManagementService.UpdateAppointment(token, updateDTO);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("get-appointment-detail")]
        public async Task<IActionResult> GetAppointmentDetail([FromQuery] Guid appointmentId)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _profileManagementService.GetAppointmentDetail(token, appointmentId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("check-review")]
        public async Task<IActionResult> CheckReview([FromQuery] Guid appointmentId)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _profileManagementService.CheckReview(token, appointmentId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("count-service")]
        public async Task<IActionResult> CountService()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _profileManagementService.CountService(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
