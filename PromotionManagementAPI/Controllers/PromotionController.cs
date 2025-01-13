using Microsoft.AspNetCore.Mvc;
using PromotionManagementAPI.DTOs.ResultModel;
using PromotionManagementAPI.Services;

namespace PromotionManagementAPI.Controllers
{
    public class PromotionController : ControllerBase
    {
        private readonly IPromotionService _service;
        public PromotionController(IPromotionService service)
        {
            _service = service;
        }
        [HttpGet("promotion-list")]
        public async Task<IActionResult> GetListPromotion(int page)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetListPromotion(token,page);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
