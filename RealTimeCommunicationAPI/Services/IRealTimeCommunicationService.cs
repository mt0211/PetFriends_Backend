using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RealTimeCommunicationAPI.DTOs.ResultModel;

namespace RealTimeCommunicationAPI.Services
{
    public interface IRealTimeCommunicationService
    {
        Task<ResultModel> SendMessage(string token, Guid receiverId, string content, string messageType = "text", string mediaUrl = null);
        
        Task<ResultModel> GetAppChatHistory(string token, Guid otherClinicId);
        Task<ResultModel> MarkMessagesAsRead(string token, Guid senderId);
        Task<ResultModel> GetUnreadMessageCount(string token);
        Task<ResultModel> GetRecentChats(string token);
        Task<ResultModel> DeleteMessage(string token, Guid messageId);
        Task<ResultModel> DeleteConversation(string token, Guid otherUserId);
        Task<ResultModel> SendMessageInternal(string senderId, string receiverId, string message);
        Task<ResultModel> GetAppChatHistoryInternal(Guid userId, Guid otherUserId);
        Task<ResultModel> DeleteMessageInternal(Guid messageId, Guid userId);
        Task<ResultModel> DeleteConversationInternal(Guid userId, Guid otherUserId);
        Task<ResultModel> DeleteMessageForSenderInternal(Guid messageId, Guid userId);
        Task<ResultModel> InitiateVideoCall(string token, Guid receiverId, string callType = "video");
        Task<ResultModel> AcceptVideoCall(string token, Guid callId);
        Task<ResultModel> RejectVideoCall(string token, Guid callId);
        Task<ResultModel> EndVideoCall(string token, Guid callId);
        Task<ResultModel> GetCallHistory(string token, int take = 20);
        Task<ResultModel> GetCallById(string token, Guid callId);

        Task<ResultModel> GetActiveCalls(string userId);

        
      
    }
}