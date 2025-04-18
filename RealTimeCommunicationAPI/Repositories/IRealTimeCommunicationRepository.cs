using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Models;

namespace RealTimeCommunicationAPI.Repositories
{
    public interface IRealTimeCommunicationRepository
    {
        Task<User> GetUserById(Guid id);
        Task<ChatMessage> SaveMessage(ChatMessage message);
        Task<List<ChatMessage>> GetAppChatHistory(Guid senderId, Guid receiverId);
        Task MarkMessageAsRead(Guid senderId, Guid receiverId);
        Task<int> GetUnreadMessageCount(Guid userId);
        Task<List<dynamic>> GetRecentChats(Guid userId);
        Task<bool> DeleteMessage(Guid messageId, Guid userId);
        Task<bool> DeleteConversation(Guid userId, Guid otherUserId);
        Task<ChatMessage> GetMessageById(Guid messageId);
        Task<bool> MarkMessageForDelete(Guid messageId, Guid userId);
       
       //Video call
        Task<VideoCall> CreateVideoCall(VideoCall videoCall);
        Task UpdateCallStatus(Guid callId, string status);
        Task AcceptCall(Guid callId);
        Task RejectCall(Guid callId);
        Task EndCall(Guid callId);
        Task<List<VideoCall>> GetUserCallHistory(Guid userId, int take = 20);
        Task<List<VideoCall>> GetActiveCalls();
        Task<VideoCall?> GetCallById(Guid callId);
    }
}