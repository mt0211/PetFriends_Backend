using AccountManagementAPI.DTOs.ResultModel;
using AccountManagementAPI.DTOs.UserDTOs;
using AccountManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace AccountManagementAPI.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        [HttpGet("account-list")]
        public async Task<IActionResult> GetAccountList(int page)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            ResultModel result = await _accountService.GetAllAccount(token, page);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("update-account-status")]
        public async Task<IActionResult> UpdateAccountStatus(UserUpdateStatusModel userUpdateStatusModel)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _accountService.UpdateAccountStatus(token, userUpdateStatusModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPost("add-new-account")]
        public async Task<IActionResult> AddNewAccount(UserAddModel userAddModel)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _accountService.AddNewAccount(token, userAddModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("account-detail")]
        public async Task<IActionResult> GetAccountDetail(Guid AccountID)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _accountService.GetAccountDetail(token, AccountID);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("update-account")]
        public async Task<IActionResult> UpdateAccount(UserUpdateModel userUpdateModel)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _accountService.UpdateAccount(token, userUpdateModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
