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
        Task<List<ChatMessage>> GetChatHistory(Guid senderId, Guid receiverId, int skip, int take);
        Task MarkMessageAsRead(Guid senderId, Guid receiverId);
        Task<int> GetUnreadMessageCount(Guid userId);
        Task<List<dynamic>> GetRecentChats(Guid userId);
        Task<bool> DeleteMessage(Guid messageId, Guid userId);
        Task<bool> DeleteConversation(Guid userId, Guid otherUserId);
        Task<ChatMessage> GetMessageById(Guid messageId);
    }
}