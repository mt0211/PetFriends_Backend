using ClinicServiceManagement.DTOs.ResultModel;
using ClinicServiceManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClinicServiceManagement.Controllers
{
    [ApiController]
    [Route("api/clinicservice")]
    public class ClinicServiceController : ControllerBase
    {
        private readonly IClinicServiceService _service;
        public ClinicServiceController(IClinicServiceService service)
        {
            _service = service;
        }
        [HttpGet("clinicservice-list")]
        public async Task<IActionResult> GetClinicServiceList()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            ResultModel result = await _service.GetAllService(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
