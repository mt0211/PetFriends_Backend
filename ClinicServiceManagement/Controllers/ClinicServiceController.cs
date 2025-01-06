using ClinicServiceManagement.DTOs.ResultModel;
using ClinicServiceManagement.DTOs.ServiceDTOs;
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
        [HttpPost("add-service")]
        public async Task<IActionResult> AddService([FromBody] ServiceAddDTO serviceAddDTO)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.AddNewService(token, serviceAddDTO);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("category-list")]
        public async Task<IActionResult> GetCategoryList()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetAllCategory(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("service-detail")]
        public async Task<IActionResult> GetServiceDetail(Guid id)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetServiceDetail(token,id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("update-service-status")]
        public async Task<IActionResult> UpdateStatus(Guid id)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.UpdateServiceStatus(token, id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("update-service")]
        public async Task<IActionResult> UpdateService([FromBody] ServiceUpdateDTO updateDTO)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.UpdateService(token, updateDTO);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

    }
}
