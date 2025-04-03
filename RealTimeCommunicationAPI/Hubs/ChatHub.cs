using DataAccess.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RealTimeCommunicationAPI.Repositories;
using RealTimeCommunicationAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RealTimeCommunicationAPI.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub
    {
        private readonly IRealTimeCommunicationService _service;
        private readonly IRealTimeCommunicationRepository _repository;

        public ChatHub(IRealTimeCommunicationService service, IRealTimeCommunicationRepository repository)
        {
            _service = service;
            _repository = repository;
        }

        // Gửi tin nhắn trực tiếp đến một người dùng cụ thể
        public async Task<object> SendMessage(string receiverId, string message)
            {
                // Log tham số nhận được
                Console.WriteLine($"[ChatHub] Received parameters: receiverId={receiverId}, message={message}");
                
                // Lấy sender id từ claim "userid"
                var senderId = Context.User?.FindFirst("userid")?.Value;
                Console.WriteLine($"[ChatHub] Extracted sender ID: {senderId}");
                
                if (senderId == null)
                {
                    Console.WriteLine("[ChatHub] ERROR: senderId is null");
                    await Clients.Caller.SendAsync("MessageError", "Invalid sender id");
                    return null; // Trả về null khi senderId là null
                }
                
                // Thay đổi: Truyền senderId trực tiếp thay vì token
                Console.WriteLine($"[ChatHub] Calling SendMessageInternal with senderId={senderId}, receiverId={receiverId}, message={message}");
                var result = await _service.SendMessageInternal(senderId, receiverId, message);
                Console.WriteLine($"[ChatHub] SendMessageInternal result: IsSuccess={result.IsSuccess}, Message={result.Message}");
                
                if(result.IsSuccess)
                {
                    // Phát tin nhắn đến client nhận với sender id
                    await Clients.Group(receiverId).SendAsync("ReceiveMessage", senderId, message);
                    // Phản hồi lại cho client gửi
                    await Clients.Caller.SendAsync("MessageSent", receiverId, message);
                    Console.WriteLine($"[ChatHub] Message sent successfully from {senderId} to {receiverId}");
                    return result.Data; // Trả về dữ liệu khi thành công
                }
                else
                {
                    await Clients.Caller.SendAsync("MessageError", result.Message);
                    Console.WriteLine($"[ChatHub] Error sending message: {result.Message}");
                    return null; // Trả về null khi thất bại
                }
            }

        //Chat history
        public async Task<List<ChatMessage>> GetChatHistory(string otherUserId)
        {
            var userId = Context.User?.FindFirst("userid")?.Value;
            Console.WriteLine($"[ChatHub] GetChatHistory called with userId={userId}, otherUserId={otherUserId}");
            
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid userGuid))
            {
                await Clients.Caller.SendAsync("MessageError", "Invalid user ID");
                return new List<ChatMessage>();
            }
            
            if (string.IsNullOrEmpty(otherUserId) || !Guid.TryParse(otherUserId, out Guid otherUserGuid))
            {
                await Clients.Caller.SendAsync("MessageError", "Invalid other user ID");
                return new List<ChatMessage>();
            }
            
            try
            {
                // Gọi service để lấy lịch sử chat
                var result = await _service.GetAppChatHistoryInternal(userGuid, otherUserGuid);
                
                if (result.IsSuccess)
                {
                    // Trả về lịch sử chat cho client gọi
                    return (List<ChatMessage>)result.Data;
                }
                else
                {
                    await Clients.Caller.SendAsync("MessageError", result.Message);
                    return new List<ChatMessage>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChatHub] Error in GetChatHistory: {ex.Message}");
                await Clients.Caller.SendAsync("MessageError", "Error retrieving chat history");
                return new List<ChatMessage>();
            }
        }
       
        // Thông báo khi người dùng online/offline
        private static readonly Dictionary<string, HashSet<string>> _userConnections = new Dictionary<string, HashSet<string>>();

            public override async Task OnConnectedAsync()
            {
                var userId = Context.User?.FindFirst("userid")?.Value;
                Console.WriteLine($"[OnConnectedAsync] User connected with userId: {userId}");
                if (userId != null)
                {
                    // Thêm connection vào danh sách của user
                    lock (_userConnections)
                    {
                        if (!_userConnections.ContainsKey(userId))
                        {
                            _userConnections[userId] = new HashSet<string>();
                            Console.WriteLine($"[OnConnectedAsync] Bắn sự kiện UserOnline cho user: {userId}");
                            // Chỉ gửi UserOnline khi đây là kết nối đầu tiên
                            _ = Clients.All.SendAsync("UserOnline", userId);
                        }
                        _userConnections[userId].Add(Context.ConnectionId);
                    }
                    
                    await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                }
                await base.OnConnectedAsync();
            }

            public override async Task OnDisconnectedAsync(Exception exception)
            {
                var userId = Context.User?.FindFirst("userid")?.Value;
                if (userId != null)
                {
                    bool shouldNotifyOffline = false;
                    
                    // Xóa connection khỏi danh sách của user
                    lock (_userConnections)
                    {
                        if (_userConnections.ContainsKey(userId))
                        {
                            _userConnections[userId].Remove(Context.ConnectionId);
                            // Chỉ gửi UserOffline khi đây là kết nối cuối cùng
                            if (_userConnections[userId].Count == 0)
                            {
                                _userConnections.Remove(userId);
                                shouldNotifyOffline = true;
                            }
                        }
                    }
                    
                    if (shouldNotifyOffline)
                    {
                        Console.WriteLine($"[OnDisconnectedAsync] Bắn sự kiện UserOffline cho user: {userId}");
                        await Clients.All.SendAsync("UserOffline", userId);
                    }
                }
                await base.OnDisconnectedAsync(exception);
            }

        public async Task GetOnlineUsers()
        {
            var onlineUsers = new List<string>();
            lock (_userConnections)
            {
                onlineUsers = _userConnections.Keys.ToList();
            }
            await Clients.Caller.SendAsync("OnlineUsers", onlineUsers);
        }
       
        // Thông báo khi người dùng đang nhập tin nhắn
        public async Task NotifyTyping(string receiverId)
        {
            var userId = Context.User?.FindFirst("userid")?.Value;
            if (userId != null)
            {
                await Clients.Group(receiverId).SendAsync("UserTyping", userId);
            }
        }
       
        // Đánh dấu tin nhắn đã đọc
        public async Task MarkAsRead(string senderId)
        {
            var userId = Context.User?.FindFirst("userid")?.Value;
            if (userId != null)
            {
                await Clients.Group(senderId).SendAsync("MessagesRead", userId);
            }
        }
       
        // Thông báo khi một tin nhắn bị xóa
        public async Task NotifyMessageDeleted(string receiverId, string messageId)
        {
            var userId = Context.User?.FindFirst("userid")?.Value;
            if (userId != null)
            {
                await Clients.Group(receiverId).SendAsync("MessageDeleted", userId, messageId);
            }
        }
       
        // Thông báo khi toàn bộ cuộc trò chuyện bị xóa
        public async Task NotifyConversationDeleted(string receiverId)
        {
            var userId = Context.User?.FindFirst("userid")?.Value;
            if (userId != null)
            {
                await Clients.Group(receiverId).SendAsync("ConversationDeleted", userId);
            }
        }
        // Xóa tin nhắn đơn lẻ
    public async Task DeleteMessage(string messageId)
    {
        // Log tham số nhận được
        Console.WriteLine($"[ChatHub] DeleteMessage called with messageId={messageId}");
        
        // Lấy sender id từ claim "userid"
        var userId = Context.User?.FindFirst("userid")?.Value;
        Console.WriteLine($"[ChatHub] Extracted user ID: {userId}");
        
        if (userId == null)
        {
            Console.WriteLine("[ChatHub] ERROR: userId is null");
            await Clients.Caller.SendAsync("DeleteMessageError", "Invalid user id");
            return;
        }

        if (string.IsNullOrEmpty(messageId) || !Guid.TryParse(messageId, out Guid messageGuid))
        {
            Console.WriteLine("[ChatHub] ERROR: Invalid messageId");
            await Clients.Caller.SendAsync("DeleteMessageError", "Invalid message ID");
            return;
        }

        try
        {
            // Lấy thông tin tin nhắn trước khi xóa để biết receiverId
            var message = await _repository.GetMessageById(messageGuid);
            if (message == null)
            {
                await Clients.Caller.SendAsync("DeleteMessageError", "Message not found");
                return;
            }

            // Xác định receiverId (người nhận tin nhắn)
            string receiverId = message.ReceiverId.ToString();
            if (message.SenderId.ToString() != userId)
            {
                // Nếu người xóa không phải người gửi, thì không cho phép
                await Clients.Caller.SendAsync("DeleteMessageError", "You don't have permission to delete this message");
                return;
            }

            // Gọi service để xóa tin nhắn
            var result = await _service.DeleteMessageInternal(messageGuid, Guid.Parse(userId));
            
            if (result.IsSuccess)
            {
                // Thông báo cho người gửi
                await Clients.Caller.SendAsync("MessageDeleted", messageId);
                
                // Thông báo cho người nhận
                await Clients.Group(receiverId).SendAsync("MessageDeleted", userId, messageId);
                
                Console.WriteLine($"[ChatHub] Message {messageId} deleted successfully by {userId}");
            }
            else
            {
                await Clients.Caller.SendAsync("DeleteMessageError", result.Message);
                Console.WriteLine($"[ChatHub] Error deleting message: {result.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatHub] Exception in DeleteMessage: {ex.Message}");
            await Clients.Caller.SendAsync("DeleteMessageError", "Error processing delete request");
        }
    }

    // Xóa toàn bộ cuộc trò chuyện
    public async Task DeleteConversation(string otherUserId)

    {
        // Log tham số nhận được
        Console.WriteLine($"[ChatHub] DeleteConversation called with otherUserId={otherUserId}");
        
        // Lấy sender id từ claim "userid"
        var userId = Context.User?.FindFirst("userid")?.Value;
        Console.WriteLine($"[ChatHub] Extracted user ID: {userId}");
        
        if (userId == null)
        {
            Console.WriteLine("[ChatHub] ERROR: userId is null");
            await Clients.Caller.SendAsync("DeleteConversationError", "Invalid user id");
            return;
        }

        if (string.IsNullOrEmpty(otherUserId) || !Guid.TryParse(otherUserId, out Guid otherUserGuid))
        {
            Console.WriteLine("[ChatHub] ERROR: Invalid otherUserId");
            await Clients.Caller.SendAsync("DeleteConversationError", "Invalid other user ID");
            return;
        }

        try
        {
            // Gọi service để xóa cuộc trò chuyện
            var result = await _service.DeleteConversationInternal(Guid.Parse(userId), otherUserGuid);
            
            if (result.IsSuccess)
            {
                // Thông báo cho người gửi
                await Clients.Caller.SendAsync("ConversationDeleted", otherUserId);
                
                // Thông báo cho người nhận
                await Clients.Group(otherUserId).SendAsync("ConversationDeleted", userId);
                
                Console.WriteLine($"[ChatHub] Conversation between {userId} and {otherUserId} deleted successfully");
            }
            else
            {
                await Clients.Caller.SendAsync("DeleteConversationError", result.Message);
                Console.WriteLine($"[ChatHub] Error deleting conversation: {result.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatHub] Exception in DeleteConversation: {ex.Message}");
            await Clients.Caller.SendAsync("DeleteConversationError", "Error processing delete request");
        }
    }


    // Xóa tin nhắn phia' người gửi
    public async Task DeleteMessageForSender(string messageId)
    {
        Console.WriteLine($"[ChatHub] DeleteMessageForSender called with messageId={messageId}");
        
        var userId = Context.User?.FindFirst("userid")?.Value;
        Console.WriteLine($"[ChatHub] Extracted user ID: {userId}");
        
        if (userId == null)
        {
            Console.WriteLine("[ChatHub] ERROR: userId is null");
            await Clients.Caller.SendAsync("DeleteMessageError", "Invalid user id");
            return;
        }
        
        if (string.IsNullOrEmpty(messageId) || !Guid.TryParse(messageId, out Guid messageGuid))
        {
            Console.WriteLine("[ChatHub] ERROR: Invalid messageId");
            await Clients.Caller.SendAsync("DeleteMessageError", "Invalid message ID");
            return;
        }
        
        try
        {
            // Gọi service để xóa tin nhắn chỉ cho người gửi
            var result = await _service.DeleteMessageForSenderInternal(messageGuid, Guid.Parse(userId));
            
            if (result.IsSuccess)
            {
                // Thông báo cho người gửi
                await Clients.Caller.SendAsync("MessageDeletedForSender", messageId);
                
                Console.WriteLine($"[ChatHub] Message {messageId} deleted for sender {userId} successfully");
            }
            else
            {
                await Clients.Caller.SendAsync("DeleteMessageError", result.Message);
                Console.WriteLine($"[ChatHub] Error deleting message: {result.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatHub] Exception in DeleteMessageForSender: {ex.Message}");
            await Clients.Caller.SendAsync("DeleteMessageError", "Error processing delete request");
        }
    }
}
}
