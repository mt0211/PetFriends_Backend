using Microsoft.AspNetCore.Mvc;
using UserAuthenticationAPI.Services;
using UserAuthenticationAPI.DTOs.ResultModel;
using UserAuthenticationAPI.DTOs.UserDTOs;
namespace UserAuthenticationAPI.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IVerifyService _verifyService;
        public UserController(UserService userService, IVerifyService verifyService)
        {
            _userService = userService;
            _verifyService = verifyService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginReqModel userLoginReqModel)
        {
            ResultModel result = await _userService.Login(userLoginReqModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPost("register")]
        public async Task<IActionResult> CreateUser([FromBody] UserReqModel Form)
        {
            ResultModel result = await _userService.CreateAccount(Form);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
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
            var result = await _userService.GetUserProfile(userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("clinic-profile")]
        public async Task<IActionResult> GetUserProfileById(Guid userId)
        {
            var result = await _userService.GetUserProfile(userId);
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
            ResultModel result = await _userService.UpdateUserProfile(updateModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordReqModel changePasswordModel)
        {
            var userIdString = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                return BadRequest("Unable to retrieve user ID");
            }

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return BadRequest("Invalid user ID format");
            }

            ResultModel result = await _userService.ChangePassword(userId, changePasswordModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] UserResetPasswordReqModel ResetPasswordReqModel)
        {
            ResultModel result = await _userService.ResetPassword(ResetPasswordReqModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("send-otp")] // for reset password
        public async Task<IActionResult> SendOtp([FromBody] UserEmailForSendOTP email)
        {
            ResultModel result = await _verifyService.SendOTPEmailRequest(email.Email);
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
        
        [HttpPost("verify-reset-password")]
        public async Task<IActionResult> VerifyResetPassword([FromBody] UserVerifyOTPResModel VerifyModel)
        {
            if (string.IsNullOrEmpty(VerifyModel.Email) || string.IsNullOrEmpty(VerifyModel.OTPCode))
            {
                return BadRequest("Email and OTP code are required.");
            }
            ResultModel result = await _verifyService.VerifyResetPassword(VerifyModel.Email, VerifyModel.OTPCode);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}