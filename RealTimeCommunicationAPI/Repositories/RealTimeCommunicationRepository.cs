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
            message.IsDeleteForSender = false;
            message.IsDeleteForReceiver = false;
            await _context.ChatMessages.AddAsync(message);
            await _context.SaveChangesAsync();
            return message;
        }
       

        public async Task<List<ChatMessage>> GetAppChatHistory(Guid senderId, Guid receiverId)
        {
            return await _context.ChatMessages
            .Where(m => 
                (m.SenderId == senderId && m.ReceiverId == receiverId && m.IsDeleteForSender == false) || 
                (m.SenderId == receiverId && m.ReceiverId == senderId && m.IsDeleteForReceiver == false))
            .OrderBy(m => m.SentTime)
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
                .Where(m => m.SenderId == userId || m.ReceiverId == userId && m.IsDeleteForSender == false && m.IsDeleteForReceiver == false)
                .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();
                
            var result = new List<dynamic>();
            
            foreach (var partnerId in chatPartners)
            {
                // Lấy tin nhắn gần nhất
                var lastMessage = await _context.ChatMessages
                    .Where(m => 
                        (m.SenderId == userId && m.ReceiverId == partnerId && m.IsDeleteForSender == false) || 
                        (m.SenderId == partnerId && m.ReceiverId == userId && m.IsDeleteForSender == false))
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

        public async Task<bool> MarkMessageForDelete(Guid messageId, Guid userId)
        {
            Console.WriteLine($"[Repository] MarkMessageForDelete called with messageId={messageId}, userId={userId}");
            
            var message = await _context.ChatMessages.FindAsync(messageId);
            
            if (message == null)
            {
                Console.WriteLine($"[Repository] Message with ID {messageId} not found");
                return false;
            }
            
            Console.WriteLine($"[Repository] Found message: SenderId={message.SenderId}, ReceiverId={message.ReceiverId}");
            Console.WriteLine($"[Repository] Current IsDeleteForSender={message.IsDeleteForSender}, IsDeleteForReceiver={message.IsDeleteForReceiver}");
            
            _context.ChatMessages.Attach(message);
            // Kiểm tra xem người dùng có quyền xóa tin nhắn không
            if (message.SenderId != userId && message.ReceiverId != userId)
            {
                Console.WriteLine($"[Repository] User {userId} doesn't have permission to delete message {messageId}");
                return false;
            }
            
            // Xác định người dùng là người gửi hay người nhận và đánh dấu tin nhắn là đã xóa cho người phù hợp
            if (message.SenderId == userId)
            {
                message.IsDeleteForSender = true;
                _context.Entry(message).Property(c => c.IsDeleteForSender).IsModified = true;
                Console.WriteLine($"[Repository] Marked message as deleted for sender");
            }
            else if (message.ReceiverId == userId)
            {
                message.IsDeleteForReceiver = true;
                _context.Entry(message).Property(c => c.IsDeleteForReceiver).IsModified = true;
                Console.WriteLine($"[Repository] Marked message as deleted for receiver");
            }
            
            // Nếu cả hai bên đều đã xóa, xóa tin nhắn khỏi DB
            if (message.IsDeleteForSender == true && message.IsDeleteForReceiver == true)
            {
                Console.WriteLine($"[Repository] Both sender and receiver have deleted the message, removing from database");
                _context.ChatMessages.Remove(message);
            }
            
            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"[Repository] Changes saved successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Repository] Error saving changes: {ex.Message}");
                return false;
            }
        }
       
    }
}