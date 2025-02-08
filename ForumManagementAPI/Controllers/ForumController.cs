using ForumManagementAPI.DTOs.ResultModel;
using ForumManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ForumManagementAPI.Controllers
{
    [ApiController]
    [Route("api/forum")]
    public class ForumController : ControllerBase
    {
        private readonly IForumService _forumService;
        public ForumController(IForumService forumService)
        {
            _forumService = forumService;
        }
        [HttpGet("post-list")]
        public async Task<IActionResult> GetPostList(int page)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            ResultModel result = await _forumService.GetAllForumPost(token, page);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("post-detail")]
        public async Task<IActionResult> GetPostDetail(Guid PostID)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            ResultModel result = await _forumService.GetPostDetail(token,PostID);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpDelete("delete-post/{id}")]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            ResultModel result = await _forumService.DeletePost(token, id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpDelete("delete-comment/{id}")]
        public async Task<IActionResult> DeleteComment(Guid id)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            ResultModel result = await _forumService.DeleteComment(token, id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
