using AdminAuthenticationAPI.DTOs.ResultModelAdmin;
using Microsoft.AspNetCore.Mvc;
using AdminAuthenticationAPI.Service;
using AdminAuthenticationAPI.Service.AdminService;
//using AdminAuthenticationAPI.Service.VerifyAdminService;
using AdminAuthenticationAPI.DTOs.AdminDTOs;
using static AdminAuthenticationAPI.DTOs.AdminDTOs.AdminReqModel;
using AdminAuthenticationAPI.DTOs.GoogleLoginDTOs;
namespace AdminAuthenticationAPI.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        // private readonly IVerifyAdminService _verifyAdminService;
        public AdminController(IAdminService adminService/*, IVerifyAdminService verifyAdminService*/)
        {
            _adminService = adminService;
            //  _verifyAdminService = verifyAdminService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginReqModel adminLoginReqModel)
        {
            ResultModelAdmin result = await _adminService.LoginAdmin(adminLoginReqModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDTO googleLoginDTO)
        {
            ResultModelAdmin result = await _adminService.LoginWithGoogle(googleLoginDTO.Token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

    }
}
