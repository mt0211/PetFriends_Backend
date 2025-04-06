using AppointmentManagementAPI.DTOs.ResultModel.AppointmentDTOs;
using AppointmentManagementAPI.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
        public async Task<IActionResult> GetAppointmentList(int page, int size)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            var result = await _appointmentService.GetAllAppointment(token, page, size);
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
            var result = await _appointmentService.UpdateAppointmentStatus(token, updatestatusmodel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("service-list")]
        public async Task<IActionResult> GetServiceList()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _appointmentService.GetListClinicservice(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("check-user")]
        public async Task<IActionResult> CheckUserByPhoneNumber(string phonenumber)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _appointmentService.CheckUserByPhoneNumber(token, phonenumber);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("check-pet")]
        public async Task<IActionResult> CheckPetByNameAndUserID(string petName, Guid UserId)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _appointmentService.CheckPetByNameAndUserID(token, petName, UserId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPost("add-appointment")]
        public async Task<IActionResult> AddAppointment([FromBody] AppointmentAddModel dto)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _appointmentService.AddAppointment(token, dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("appointment-detail")]
        public async Task<IActionResult> GetAppointmentDetail(Guid AppointmentID)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _appointmentService.GetAppointmentDetail(token, AppointmentID);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpDelete("delete-appointment/{id}")]
        public async Task<IActionResult> DeleteAppointment(Guid id)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _appointmentService.DeleteAppointment(token, id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("update-appointment")]
        public async Task<IActionResult> UpdateAppointment([FromBody] AppointmentUpdateModel appointmentUpdate)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _appointmentService.UpdateAppointment(token, appointmentUpdate);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("user-pets")]
        public async Task<IActionResult> GetUserPets([FromQuery] string? phone, [FromQuery] string? email)
        {
            var result = await _appointmentService.GetPetsByPhoneOrEmail(phone, email);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
