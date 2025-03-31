using DataAccess.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
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

        public ChatHub(IRealTimeCommunicationService service)
        {
            _service = service;
        }

        // Gửi tin nhắn trực tiếp đến một người dùng cụ thể
        public async Task SendMessage(string receiverId, string message)
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
                return;
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
            }
            else
            {
                await Clients.Caller.SendAsync("MessageError", result.Message);
                Console.WriteLine($"[ChatHub] Error sending message: {result.Message}");
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
    }
}
