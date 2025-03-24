using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RealTimeCommunicationAPI.Services;
using RealTimeCommunicationAPI.DTOs.MessageDTOs;

namespace RealTimeCommunicationAPI.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IRealTimeCommunicationService _service;
        
        public ChatHub(IRealTimeCommunicationService service)
        {
            _service = service;
        }
        
        // Gửi tin nhắn trực tiếp đến một người dùng cụ thể
        public async Task SendMessage(string receiverId, string message, string messageType = "text", string mediaUrl = null)
        {
            var senderId = Context.UserIdentifier;
            
            if (Guid.TryParse(senderId, out Guid senderGuid) && Guid.TryParse(receiverId, out Guid receiverGuid))
            {
                // Lấy token từ context
                string token = Context.GetHttpContext().Request.Headers["Authorization"].ToString().Split(" ")[1];
                
                // Lưu tin nhắn vào database
                var result = await _service.SendMessage(token, senderGuid, receiverGuid, message, messageType, mediaUrl);
                
                if (result.IsSuccess)
                {
                    // Gửi tin nhắn đến người nhận
                    await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message, messageType, mediaUrl);
                    
                    // Phản hồi lại cho người gửi để xác nhận tin nhắn đã được gửi
                    await Clients.Caller.SendAsync("MessageSent", receiverId, message, messageType, mediaUrl);
                }
            }
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
            if (Guid.TryParse(senderId, out Guid senderGuid))
            {
                string token = Context.GetHttpContext().Request.Headers["Authorization"].ToString().Split(" ")[1];
                var result = await _service.MarkMessagesAsRead(token, senderGuid);
                
                if (result.IsSuccess)
                {
                    await Clients.User(senderId).SendAsync("MessagesRead", Context.UserIdentifier);
                }
            }
        }
    }
}