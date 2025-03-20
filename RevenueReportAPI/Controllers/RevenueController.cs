using Microsoft.AspNetCore.Mvc;
using RevenueReportAPI.DTOs.ResultModel;
using RevenueReportAPI.Services;

namespace RevenueReportAPI.Controllers
{
    [ApiController]
    [Route("api/revenue")]
    public class RevenueController : ControllerBase
    {
        private readonly IRevenueService _service;
        public RevenueController(IRevenueService service)
        {
            _service = service;
        }
        [HttpGet("user-booking-summary")]
        public async Task<IActionResult> GetUserBookingSummaries()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetUserBookingSummary(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("detail-service-revenue")]
        public async Task<IActionResult> GetDetailServiceRevenue([FromHeader] int? year, [FromHeader] string? month)
        {
            int? monthValue = null;
            if (!string.IsNullOrEmpty(month))
            {
                if (int.TryParse(month, out int parsedMonth) && parsedMonth >= 1 && parsedMonth <= 12)
                {
                    monthValue = parsedMonth;
                }
                else
                {
                    return BadRequest(new { isSuccess = false, code = 400, message = "Month must be between 1 and 12" });
                }
            }
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var requestModel = new RevenueRequestModel
            {
                Year = year,
                Month = monthValue
            };

            ResultModel result = await _service.GetDetailServiceRevenue(token, requestModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("total-revenue")]
        public async Task<IActionResult> GetTotalRevenue([FromHeader] int? year, [FromHeader] string? month)
        {
            int? monthValue = null;
            if (!string.IsNullOrEmpty(month))
            {
                if (int.TryParse(month, out int parsedMonth) && parsedMonth >= 1 && parsedMonth <= 12)
                {
                    monthValue = parsedMonth;
                }
                else
                {
                    return BadRequest(new { isSuccess = false, code = 400, message = "Month must be between 1 and 12" });
                }
            }
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var requestModel = new RevenueRequestModel
            {
                Year = year,
                Month = monthValue
            };
            
            ResultModel result = await _service.GetTotalRevenue(token, requestModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("service-revenue")]
        public async Task<IActionResult> GetServiceRevenue([FromHeader] int? year, [FromHeader] string? month)
        {
            int? monthValue = null;
            if (!string.IsNullOrEmpty(month))
            {
                if (int.TryParse(month, out int parsedMonth) && parsedMonth >= 1 && parsedMonth <= 12)
                {
                    monthValue = parsedMonth;
                }
                else
                {
                    return BadRequest(new { isSuccess = false, code = 400, message = "Month must be between 1 and 12" });
                }
            }
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var requestModel = new RevenueRequestModel
            {
                Year = year,
                Month = monthValue
            };

            ResultModel result = await _service.GetServiceRevenue(token, requestModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportDataToExcel(int year, int? month)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetAllDataForExport(token, year, month);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
