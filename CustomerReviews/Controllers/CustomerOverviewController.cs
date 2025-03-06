using CustomerReviews.DTOs.ResultModel;
using CustomerReviews.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomerReviews.Controllers
{
    [ApiController]
    [Route("api/customeroverview")]
    public class CustomerOverviewController : ControllerBase
    {
        private readonly ICustomerOverviewService _service;
        public CustomerOverviewController(ICustomerOverviewService service)
        {
            _service = service;
        }
        [HttpGet("review-list")]
        public async Task<IActionResult> GetFeedbackList()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            ResultModel result = await _service.GetListReviews(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("rating-overview")]
        public async Task<IActionResult> GetRatingOverview()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            ResultModel result = await _service.GetRatingOverView(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
