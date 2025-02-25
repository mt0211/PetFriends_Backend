using AppForumManamgement.DTOs.PostDTOs;
using AppForumManamgement.DTOs.ResultModel;
using AppForumManamgement.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppForumManamgement.Controllers
{
    [ApiController]
    [Route("api/userforumpost")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _service;
        public PostController(IPostService service)
        {
            _service = service;
        }
        [HttpGet("post-list")]
        public async Task<IActionResult> GetListPost()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetListPost(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("reaction")]
        public async Task<IActionResult> UpdatePostReaction([FromBody] PostReactionReqModel request)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.UpdatePostReaction(token, request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("post-detail")]
        public async Task<IActionResult> GetPostByID(Guid pid)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetPostByID(token, pid);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPost("comment")]
        public async Task<IActionResult> CommentPost([FromBody] CommentPostReqModel addmodel)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.CommentPost(token, addmodel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPost("add-post")]
        public async Task<IActionResult> AddPost([FromBody] AddPostReqModel addmodel)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.AddPost(token, addmodel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("user-info")]
        public async Task<IActionResult> GetUserByID()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetUserByID(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpDelete("delete-post/{pid}")]
        public async Task<IActionResult> DeletePost(Guid pid)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.DeletePost(token, pid);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("post-detail-to-update/{pid}")]
        public async Task<IActionResult> GetPostDetailToUpdate(Guid pid)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetPostDetailToUpdate(token, pid);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("update-post")]
        public async Task<IActionResult> UpdatePost([FromBody] UpdatePostReqModel updateModel)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.UpdatePost(token, updateModel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
