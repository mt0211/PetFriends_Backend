using AppUserAuthenticationAPI.DTOs.AppUserDTOs;
using AppUserAuthenticationAPI.DTOs.GoogleLoginDTOs;
using AppUserAuthenticationAPI.DTOs.ResultModel;
using AppUserAuthenticationAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppUserAuthenticationAPI.Controllers
{
    [ApiController]
    [Route("api/appuser")]
    public class AppUserAuthenticationController : ControllerBase
    {
       

        private readonly IAppUserAuthenticationService _appUserAuthenticationService;
        private readonly IVerifyService _verifyService;

        public AppUserAuthenticationController(IAppUserAuthenticationService userService , IVerifyService verifyService)
        {
            _appUserAuthenticationService = userService; 
            _verifyService = verifyService;

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginReqModel userLoginReqModel)
        {
            ResultModel result = await _appUserAuthenticationService.Login(userLoginReqModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPost("register")]
        public async Task<IActionResult> CreateUser([FromBody] UserReqModel Form)
        {

            ResultModel result = await _appUserAuthenticationService.CreateAccount(Form);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] UserVerifyOTPResModel VerifyModel)
        {
            if (string.IsNullOrEmpty(VerifyModel.Email) || string.IsNullOrEmpty(VerifyModel.OTPCode))
            {
                return BadRequest("Email and OTP code are required.");
            }
            ResultModel result = await _verifyService.VerifyEmail(VerifyModel.Email, VerifyModel.OTPCode);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDTO googleLoginDTO)
        {
            ResultModel result = await _appUserAuthenticationService.LoginWithGoogle(googleLoginDTO.Token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPost("google-signup")]
        public async Task<IActionResult> GoogleSignUp([FromBody] GoogleLoginDTO googleLoginDTO)
        {
            if (string.IsNullOrEmpty(googleLoginDTO.Token))
            {
                return BadRequest(new { message = "Token is required" });
            }
            ResultModel result = await _appUserAuthenticationService.SignUpWithGoogle(googleLoginDTO.Token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("update-fullname")]
        public async Task<IActionResult> UpdateUserFullName([FromBody] UpdateUserFullName userUpdateFullNameModel)
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
            ResultModel result = await _appUserAuthenticationService.UpdateUserFullName(userUpdateFullNameModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
