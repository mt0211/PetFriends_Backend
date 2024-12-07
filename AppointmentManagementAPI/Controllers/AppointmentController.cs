using AppointmentManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentManagementAPI.Controllers
{
    [ApiController]
    [Route("api/appointment")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }
        [HttpGet("appointment-list")]
        public async Task<IActionResult> GetAppointmentList(int page)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _appointmentService.GetAllAppointment(token,page);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
