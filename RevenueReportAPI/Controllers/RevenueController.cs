using Microsoft.AspNetCore.Mvc;
using RevenueReportAPI.DTOs.ResultModel;
using RevenueReportAPI.Services;

namespace RevenueReportAPI.Controllers
{
    [ApiController]
    [Route("api/revenue")]
    public class RevenueController : ControllerBase
    {
        private readonly IRevenueService _service;
        public RevenueController(IRevenueService service)
        {
            _service = service;
        }
        [HttpGet("user-booking-summary")]
        public async Task<IActionResult> GetUserBookingSummaries()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetUserBookingSummary(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("service-revenue")]
        public async Task<IActionResult> GetServiceRevenue([FromQuery] RevenueRequestModel request)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetServiceRevenue(token, request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("total-revenue")]
        public async Task<IActionResult> GetTotalRevenue([FromQuery] RevenueRequestModel request)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetTotalRevenue(token, request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
