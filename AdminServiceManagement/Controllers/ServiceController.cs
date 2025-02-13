using AdminServiceManagement.DTOs.ResultModel.CustomerReviews.DTOs.ResultModel;
using AdminServiceManagement.DTOs.ServiceDTOs;
using AdminServiceManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdminServiceManagement.Controllers
{
    [ApiController]
    [Route("api/adminservice")]
    public class ServiceController : ControllerBase
    {
        private readonly IServiceService _service;
        public ServiceController(IServiceService service)
        {
            _service = service;
        }

        [HttpGet("service-list")]
        public async Task<IActionResult> GetAllService(int page)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            ResultModel result = await _service.GetAllService(token, page);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("update-service-status")]
        public async Task<IActionResult> UpdateServiceStatus(ServiceUpdateRequestModel serviceUpdate)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            ResultModel result = await _service.UpdateServiceStatus(token, serviceUpdate);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
