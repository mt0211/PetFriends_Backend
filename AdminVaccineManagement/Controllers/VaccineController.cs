using AdminVaccineManagement.DTOs.ResultModel;
using AdminVaccineManagement.DTOs.VaccineDTOs;
using AdminVaccineManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdminVaccineManagement.Controllers
{
    [ApiController]
    [Route("api/vaccine")]
    public class VaccineController : ControllerBase
    {
        private readonly IVaccineService _service;
        public VaccineController(IVaccineService service)
        {
            _service = service;
        }
        [HttpGet("vaccine-list")]
        public async Task<IActionResult> GetListVaccine(int page)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            ResultModel result = await _service.GetListVaccines(token, page);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("vaccine-detail")]
        public async Task<IActionResult> GetVaccineDetail(Guid VaccineID)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            ResultModel result = await _service.GetVaccineDetail(token, VaccineID);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("update-vaccine-status")]
        public async Task<IActionResult> UpdateVaccineStatus(VaccineUpdateStatusReqModel UpdateModel)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            ResultModel result = await _service.UpdateVaccineStatus(token, UpdateModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
