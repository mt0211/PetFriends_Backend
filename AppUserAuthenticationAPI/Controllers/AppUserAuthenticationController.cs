using AppUserAuthenticationAPI.DTOs.AppUserDTOs;
using AppUserAuthenticationAPI.DTOs.ResultModel;
using AppUserAuthenticationAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppUserAuthenticationAPI.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class AppUserAuthenticationController : ControllerBase
    {
       

        private readonly IAppUserAuthenticationService _appUserAuthenticationService;
        public AppUserAuthenticationController(IAppUserAuthenticationService userService)
        {
            _appUserAuthenticationService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginReqModel userLoginReqModel)
        {
            ResultModel result = await _appUserAuthenticationService.Login(userLoginReqModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


    }
}
