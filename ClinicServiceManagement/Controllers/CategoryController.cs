using ClinicServiceManagement.DTOs.ResultModel;
using ClinicServiceManagement.DTOs.ServiceDTOs;
using ClinicServiceManagementAPI.DTOs.CategoryDTOs;
using ClinicServiceManagementAPI.Services.CategoryServices;
using Microsoft.AspNetCore.Mvc;

namespace ClinicServiceManagementAPI.Controllers
{
    [ApiController]
    [Route("api/category")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;
        public CategoryController(ICategoryService service)
        {
            _service = service;
        }
        [HttpPost("add-category")]
        public async Task<IActionResult> AddNewCategory([FromBody] CategoryAddModel categoryAddModel )
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.AddNewService(token, categoryAddModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("update-category")]
        public async Task<IActionResult> UpdateCategoryName([FromBody] CategoryUpdateModel categoryUpdateModel)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.UpdateCategoryName(token, categoryUpdateModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("category-list")]
        public async Task<IActionResult> GetListCategory()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetListCategory(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
