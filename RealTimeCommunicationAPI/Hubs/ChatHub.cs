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
            var senderId = Context.UserIdentifier;
            
            // Gửi tin nhắn đến người nhận
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message);
            
            // Phản hồi lại cho người gửi để xác nhận tin nhắn đã được gửi
            await Clients.Caller.SendAsync("MessageSent", receiverId, message);
        }
        
        // Thông báo khi người dùng online/offline
         public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            await Clients.Others.SendAsync("UserOnline", userId);
        }
        await base.OnConnectedAsync();
    }
        
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Clients.Others.SendAsync("UserOffline", Context.UserIdentifier);
            await base.OnDisconnectedAsync(exception);
        }
        
        // Thông báo khi người dùng đang nhập tin nhắn
        public async Task NotifyTyping(string receiverId)
        {
            await Clients.User(receiverId).SendAsync("UserTyping", Context.UserIdentifier);
        }
        
        // Đánh dấu tin nhắn đã đọc
        public async Task MarkAsRead(string senderId)
        {
            await Clients.User(senderId).SendAsync("MessagesRead", Context.UserIdentifier);
        }
        
        // Thông báo khi một tin nhắn bị xóa
        public async Task NotifyMessageDeleted(string receiverId, string messageId)
        {
            var senderId = Context.UserIdentifier;
            await Clients.User(receiverId).SendAsync("MessageDeleted", senderId, messageId);
        }
        
        // Thông báo khi toàn bộ cuộc trò chuyện bị xóa
        public async Task NotifyConversationDeleted(string receiverId)
        {
            var senderId = Context.UserIdentifier;
            await Clients.User(receiverId).SendAsync("ConversationDeleted", senderId);
        }
    }
}