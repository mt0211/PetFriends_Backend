using AdminReport.DTO.ResultModel;
using AdminReport.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdminReport.Controllers
{
    [ApiController]
    [Route("api/report")]
    public class AdminReportController : ControllerBase
    {
        private readonly IAdminReportService _service;
        public AdminReportController(IAdminReportService service)
        {
            _service = service;
        }
        [HttpGet("user-status")]
        public async Task<IActionResult> GetUserStatus(int year, int month)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetDataUserStatus(token, year, month);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("new-user")]
        public async Task<IActionResult> GetNewUser(int year, int month)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetDataNewUser(token, year, month);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("post-distribution")]
        public async Task<IActionResult> GetPostDistribution()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetPostDistribution(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
