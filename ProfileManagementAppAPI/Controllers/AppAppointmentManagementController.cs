using Microsoft.AspNetCore.Mvc;
using ProfileManagementAppAPI.Services;
using ProfileManagementAppAPI.DTOs;

using DataAccess.Models;
using ProfileManagementAppAPI.DTOs.ResultModel;
using AppAppointmentManagementAPI.DTOs.ReviewModel;
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
    }
}
