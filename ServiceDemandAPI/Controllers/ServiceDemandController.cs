using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServiceDemandAPI.Services;

namespace ServiceDemandAPI.Controllers
{
    [ApiController]
    [Route("api/servicedemand")]
    public class ServiceDemandController : ControllerBase
    {
        private readonly IServiceDemandService _service;

        public ServiceDemandController(IServiceDemandService service)
        {
            _service = service;
        }

       [HttpGet("getdata/{dayOfWeek}")]
        public async Task<IActionResult> GetAppointmentDemand(string dayOfWeek)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _service.GetAppointmentDemand(token,dayOfWeek);
            return StatusCode(result.Code, result);
        }
    }
}