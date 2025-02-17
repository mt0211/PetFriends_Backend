using Microsoft.AspNetCore.Mvc;
using ProfileManagementAppAPI.Services;
using ProfileManagementAppAPI.DTOs;
using ProfileManagementAppAPI.DTOs.ClinicProfileModel;

using DataAccess.Models;
using ProfileManagementAppAPI.DTOs.ResultModel;
namespace ProfileManagementAppAPI.Controllers
{



    [ApiController]
    [Route("api/profilemanagement")]

    public class ProfileManagementController : ControllerBase
    {
        private readonly IProfileManagementService _profileManagementService;

        public ProfileManagementController(IProfileManagementService profileManagementService)
        {
            _profileManagementService = profileManagementService;
        }

        [HttpGet("view-clinic")]
        public async Task<IActionResult> GetClinicList(int page)
        {
            var userIdString = User.FindFirst("userid")?.Value;
            if (string.IsNullOrEmpty(userIdString))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return BadRequest("Invalid user ID format");
            }
            var result = await _profileManagementService.GetUserProfile(userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UserUpdateModel updateModel)
        {
            var userIdString = User.FindFirst("userid")?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                return BadRequest("Unable to retrieve user ID");
            }

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return BadRequest("Invalid user ID format");
            }
            ResultModel result = await _profileManagementService.UpdateUserProfile(updateModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


    }
}
