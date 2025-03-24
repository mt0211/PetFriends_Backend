using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace RealTimeCommunicationAPI.Repositories
{
    public class RealTimeCommunicationRepository : IRealTimeCommunicationRepository
    {
        private readonly PetfriendsContext _context;
        public RealTimeCommunicationRepository(PetfriendsContext context) 
        {
            _context = context;
        }
        public async Task<User> GetUserById(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }
        public async Task<ChatMessage> SaveMessage(ChatMessage message)
        {
            message.Id = Guid.NewGuid();
            message.CreatedAt = DateTime.UtcNow;
            message.IsRead = false;
            await _context.ChatMessages.AddAsync(message);
            await _context.SaveChangesAsync();
            return message;
        }
        public async Task<List<ChatMessage>> GetChatHistory(Guid senderId, Guid receiverId, int skip, int take)
        {
            return await _context.ChatMessages
            .Where(m => 
                (m.SenderId == senderId && m.ReceiverId == receiverId) || 
                (m.SenderId == receiverId && m.ReceiverId == senderId))
            .OrderByDescending(m => m.SentTime)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        }
        public async Task MarkMessageAsRead(Guid senderId, Guid receiverId)
        {
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsRead)
                .ToListAsync();
                
            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }
            
            await _context.SaveChangesAsync();
        }
        public async Task<int> GetUnreadMessageCount(Guid userId)
        {
            return await _context.ChatMessages
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
        }
        public async Task<List<dynamic>> GetRecentChats(Guid userId)
        {
            // Lấy danh sách người dùng đã chat với userId
            var chatPartners = await _context.ChatMessages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();
                
            var result = new List<dynamic>();
            
            foreach (var partnerId in chatPartners)
            {
                // Lấy tin nhắn gần nhất
                var lastMessage = await _context.ChatMessages
                    .Where(m => 
                        (m.SenderId == userId && m.ReceiverId == partnerId) || 
                        (m.SenderId == partnerId && m.ReceiverId == userId))
                    .OrderByDescending(m => m.SentTime)
                    .FirstOrDefaultAsync();
                    
                // Lấy số tin nhắn chưa đọc
                var unreadCount = await _context.ChatMessages
                    .CountAsync(m => m.SenderId == partnerId && m.ReceiverId == userId && !m.IsRead);
                    
                // Lấy thông tin người dùng
                var partner = await _context.Users.FindAsync(partnerId);
                
                if (lastMessage != null && partner != null)
                {
                    result.Add(new
                    {
                        UserId = partnerId,
                        UserName = partner.FullName,
                        UserAvatar = partner.AvatarUrl,
                        LastMessage = lastMessage.Content,
                        LastMessageType = lastMessage.MessageType,
                        LastMessageTime = lastMessage.SentTime,
                        UnreadCount = unreadCount,
                        IsOnline = false // Sẽ được cập nhật từ SignalR
                    });
                }
            }
            
            // Sắp xếp theo thời gian tin nhắn gần nhất
            return result.OrderByDescending(c => ((dynamic)c).LastMessageTime).ToList();
        }
        public async Task<bool> DeleteMessage(Guid messageId, Guid userId)
        {
            var message = await _context.ChatMessages.FindAsync(messageId);
            
            if (message == null)
            {
                return false;
            }
            
            // Kiểm tra xem người dùng có quyền xóa tin nhắn không (chỉ người gửi mới có quyền xóa)
            if (message.SenderId != userId)
            {
                return false;
            }
            
            // Xóa hoàn toàn tin nhắn khỏi cơ sở dữ liệu
            _context.ChatMessages.Remove(message);
            
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteConversation(Guid userId, Guid otherUserId)
        {
            try
            {
                // Lấy tất cả tin nhắn giữa hai người dùng
                var messages = await _context.ChatMessages
                    .Where(m => 
                        (m.SenderId == userId && m.ReceiverId == otherUserId) || 
                        (m.SenderId == otherUserId && m.ReceiverId == userId))
                    .ToListAsync();
                    
                if (messages.Count == 0)
                {
                    return false;
                }
                
                // Xóa hoàn toàn tất cả tin nhắn
                _context.ChatMessages.RemoveRange(messages);
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<ChatMessage> GetMessageById(Guid messageId)
        {
            return await _context.ChatMessages.FindAsync(messageId);
        }
    }
}