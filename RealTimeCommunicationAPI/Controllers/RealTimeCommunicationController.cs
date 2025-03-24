using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RealTimeCommunicationAPI.DTOs.MessageDTOs;
using RealTimeCommunicationAPI.DTOs.ResultModel;
using RealTimeCommunicationAPI.Services;

namespace RealTimeCommunicationAPI.Controllers
{
    [ApiController]
    [Route("api/communication")]
    public class RealTimeCommunicationController : ControllerBase
    {
        private readonly IRealTimeCommunicationService _service;
        public RealTimeCommunicationController(IRealTimeCommunicationService service)
        {
            _service = service;
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageReqMdel model)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.SendMessage(token, model.SenderId, model.ReceiverId, model.Content, model.MessageType, model.MediaUrl);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        
        [HttpGet("chat-history/{otherUserId}")]
        public async Task<IActionResult> GetChatHistory(Guid otherUserId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            
            ResultModel result = await _service.GetChatHistory(token, otherUserId, page, pageSize);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        
        [HttpPut("mark-as-read/{senderId}")]
        public async Task<IActionResult> MarkMessagesAsRead(Guid senderId)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            
            ResultModel result = await _service.MarkMessagesAsRead(token, senderId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadMessageCount()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            
            ResultModel result = await _service.GetUnreadMessageCount(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        
        [HttpGet("recent-chats")]
        public async Task<IActionResult> GetRecentChats()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            
            ResultModel result = await _service.GetRecentChats(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpDelete("delete-message/{messageId}")]
        public async Task<IActionResult> DeleteMessage(Guid messageId)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            
            ResultModel result = await _service.DeleteMessage(token, messageId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpDelete("delete-conversation/{otherUserId}")]
        public async Task<IActionResult> DeleteConversation(Guid otherUserId)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Unable to retrieve user ID");
            }
            
            ResultModel result = await _service.DeleteConversation(token, otherUserId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}