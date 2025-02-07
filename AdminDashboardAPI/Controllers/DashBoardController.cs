using DashboardAPI.DTOs.ResultModel;
using DashboardAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardAPI.Controllers
{
    [ApiController]
    [Route("api/admindashboard")]
    public class DashBoardController : ControllerBase
    {
        private readonly IDashboardService _service;
        public DashBoardController(IDashboardService service)
        {
            _service = service;
        }
        [HttpGet("overview")]
        public async Task<IActionResult> GetDataCount()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetData(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("forum-post-statistic")]
        public async Task<IActionResult> GetForumPostStatistic([FromQuery] DateTime? date)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetForumPostStatistic(token,date);
            return Ok(result);
        }
    }
}
