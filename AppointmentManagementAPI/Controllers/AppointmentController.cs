using AppointmentManagementAPI.DTOs.ResultModel.AppointmentDTOs;
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
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            var result = await _appointmentService.GetAllAppointment(token,page);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("update-appointment-status")]
        public async Task<IActionResult> UpdateAppointmentStatus([FromBody] AppointmentUpdateStatusModel updatestatusmodel)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            var result = await _appointmentService.UpdateAppointmentStatus(updatestatusmodel);
            return result.IsSuccess ? Ok(result) : BadRequest(result) ;
        }
    }
}
