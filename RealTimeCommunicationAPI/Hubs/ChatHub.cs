using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealTimeCommunicationAPI.Hubs
{
    public class ChatHub : Hub
    {
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
            await Clients.Others.SendAsync("UserOnline", Context.UserIdentifier);
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
    }
}