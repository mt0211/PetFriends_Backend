using Microsoft.AspNetCore.Mvc;
using ProfileManagementAppAPI.Services;
using ProfileManagementAppAPI.DTOs;
using ProfileManagementAppAPI.DTOs.ClinicProfileModel;

using DataAccess.Models;
using ProfileManagementAppAPI.DTOs.ResultModel;
namespace ProfileManagementAppAPI.Controllers
{



    [ApiController]
    [Route("api/appappointmentmanagement")]

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


    }
}
