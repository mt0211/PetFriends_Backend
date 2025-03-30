using DataAccess.Models;
using RealTimeCommunicationAPI.DTOs.ResultModel;
using RealTimeCommunicationAPI.Repositories;
using RealTimeCommunicationAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealTimeCommunicationAPI.Services
{
   
    
    public class RealTimeCommunicationService : IRealTimeCommunicationService
    {
        private readonly IRealTimeCommunicationRepository _repository;
        public RealTimeCommunicationService(IRealTimeCommunicationRepository repository)
        {
            _repository = repository;
        }
        
        public async Task<ResultModel> SendMessage(string token, Guid receiverId, string content, string messageType = "text", string mediaUrl = null)
        {
            var Result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                Result.IsSuccess = false;
                Result.Code = 400; // Bad request
                Result.Message = "Invalid user ID";
                return Result;
            }
            if (userId == null)
            {
                Result.IsSuccess = false;
                Result.Code = 400; // Bad request
                Result.Message = "Please authorize";
                return Result;
            }
            try
            {
                var message = new ChatMessage
                {
                    SenderId = id,
                    ReceiverId = receiverId,
                    Content = content,
                    SentTime = DateTime.UtcNow.AddHours(7),
                    IsRead = false,
                    MessageType = messageType ?? "text",
                    MediaUrl = mediaUrl,
                    CreatedAt = DateTime.UtcNow.AddHours(7)                    
                };
                var SaveMessage = await _repository.SaveMessage(message);
                Result.IsSuccess = true;
                Result.Code = 200;
                Result.Data = SaveMessage;
                Result.Message = "Message sent successfully";
            } catch (Exception e)
        {
            Result.IsSuccess = false;
            Result.Code = 400;
            Result.ResponseFailed = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
        }
        return Result;
        }
        
        public async Task<ResultModel> GetChatHistory(string token, Guid otherUserId, int page, int pageSize)
        {
            var Result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.Message = "Invalid user ID";
                return Result;
            }
            
            try
            {
                var skip = (page - 1) * pageSize;
                var messages = await _repository.GetChatHistory(id, otherUserId, skip, pageSize);
                
                Result.IsSuccess = true;
                Result.Code = 200;
                Result.Data = messages;
                Result.Message = "Chat history retrieved successfully";
            }
            catch (Exception e)
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.ResponseFailed = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            
            return Result;
        }

        public async Task<ResultModel> GetAppChatHistory(string token, Guid otherClinicId)
        {
            var Result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.Message = "Invalid user ID";
                return Result;
            }
            
            try
            {
                
                var messages = await _repository.GetAppChatHistory(id, otherClinicId);
                
                Result.IsSuccess = true;
                Result.Code = 200;
                Result.Data = messages;
                Result.Message = "Chat history retrieved successfully";
            }
            catch (Exception e)
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.ResponseFailed = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            
            return Result;
        }
        
        public async Task<ResultModel> MarkMessagesAsRead(string token, Guid senderId)
        {
            var Result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid receiverId))
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.Message = "Invalid user ID";
                return Result;
            }
            
            try
            {
                await _repository.MarkMessageAsRead(senderId, receiverId);
                
                Result.IsSuccess = true;
                Result.Code = 200;
                Result.Message = "Messages marked as read successfully";
            }
            catch (Exception e)
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.ResponseFailed = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            
            return Result;
        }
        
        public async Task<ResultModel> GetUnreadMessageCount(string token)
        {
            var Result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.Message = "Invalid user ID";
                return Result;
            }
            
            try
            {
                var count = await _repository.GetUnreadMessageCount(id);
                
                Result.IsSuccess = true;
                Result.Code = 200;
                Result.Data = count;
                Result.Message = "Unread message count retrieved successfully";
            }
            catch (Exception e)
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.ResponseFailed = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            
            return Result;
        }
        
        public async Task<ResultModel> GetRecentChats(string token)
        {
            var Result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.Message = "Invalid user ID";
                return Result;
            }
            
            try
            {
                var chats = await _repository.GetRecentChats(id);
                
                Result.IsSuccess = true;
                Result.Code = 200;
                Result.Data = chats;
                Result.Message = "Recent chats retrieved successfully";
            }
            catch (Exception e)
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.ResponseFailed = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            
            return Result;
        }
        public async Task<ResultModel> DeleteMessage(string token, Guid messageId)
        {
            var Result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.Message = "Invalid user ID";
                return Result;
            }
            
            try
            {
                // Kiểm tra xem tin nhắn có tồn tại không
                var message = await _repository.GetMessageById(messageId);
                if (message == null)
                {
                    Result.IsSuccess = false;
                    Result.Code = 404;
                    Result.Message = "Message not found";
                    return Result;
                }
                
                // Kiểm tra xem người dùng có quyền xóa tin nhắn không
                if (message.SenderId != id)
                {
                    Result.IsSuccess = false;
                    Result.Code = 403;
                    Result.Message = "You don't have permission to delete this message";
                    return Result;
                }
                
                // Thực hiện xóa tin nhắn
                var success = await _repository.DeleteMessage(messageId, id);
                
                if (success)
                {
                    Result.IsSuccess = true;
                    Result.Code = 200;
                    Result.Message = "Message deleted successfully";
                }
                else
                {
                    Result.IsSuccess = false;
                    Result.Code = 400;
                    Result.Message = "Failed to delete message";
                }
            }
            catch (Exception e)
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.ResponseFailed = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            
            return Result;
        }
        public async Task<ResultModel> DeleteConversation(string token, Guid otherUserId)
        {
            var Result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.Message = "Invalid user ID";
                return Result;
            }
            
            try
            {
                // Kiểm tra xem người dùng kia có tồn tại không
                var otherUser = await _repository.GetUserById(otherUserId);
                if (otherUser == null)
                {
                    Result.IsSuccess = false;
                    Result.Code = 404;
                    Result.Message = "User not found";
                    return Result;
                }
                
                // Thực hiện xóa cuộc trò chuyện
                var success = await _repository.DeleteConversation(id, otherUserId);
                
                if (success)
                {
                    Result.IsSuccess = true;
                    Result.Code = 200;
                    Result.Message = "Conversation deleted successfully";
                }
                else
                {
                    Result.IsSuccess = false;
                    Result.Code = 404;
                    Result.Message = "No messages found or failed to delete conversation";
                }
            }
            catch (Exception e)
            {
                Result.IsSuccess = false;
                Result.Code = 400;
                Result.ResponseFailed = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            
            return Result;
        }
    }
}