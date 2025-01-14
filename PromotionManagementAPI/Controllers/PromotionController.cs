using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using PromotionManagementAPI.DTOs.PromotionDTOs;
using PromotionManagementAPI.DTOs.ResultModel;
using PromotionManagementAPI.Services;

namespace PromotionManagementAPI.Controllers
{
    [ApiController]
    [Route("api/promotion")]
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
        [HttpGet("category-list")]
        public async Task<IActionResult> GetCategoryList()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetAllCategory(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpDelete("delete-promotion/{PromotionID}")]
        public async Task<IActionResult> DeletePromotion(Guid PromotionID)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.DeletePromotion(token, PromotionID);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("add-promotion")]
        public async Task<IActionResult> AddPromotion([FromBody] PromotionAddModel promotionAddModel)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.AddPromotion(token, promotionAddModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("update-promotion")]
        public async Task<IActionResult> UpdatePromotion([FromBody] PromotionUpdateModel promotionUpdateModel)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.UpdatePromotion(token, promotionUpdateModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
