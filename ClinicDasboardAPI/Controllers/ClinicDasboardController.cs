using ClinicDasboardAPI.DTOs.ResultModel;
using ClinicDasboardAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClinicDasboardAPI.Controllers
{
    [ApiController]
    [Route("api/clinicdashboard")]
    public class ClinicDasboardController : ControllerBase
    {
        private readonly IClinicDashboardService _service;
        public ClinicDasboardController(IClinicDashboardService service)
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
        [HttpGet("appointment-statistic")]
        public async Task<IActionResult> GetAppointmentStatistic([FromQuery] DateTime? date)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetAppointmentStatistic(token,date);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
