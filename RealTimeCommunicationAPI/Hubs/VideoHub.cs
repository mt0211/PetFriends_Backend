using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealTimeCommunicationAPI.Hubs
{
    public class VideoHub : Hub
    {
      // Bắt đầu cuộc gọi với một người dùng
        public async Task InitiateCall(string receiverId)
        {
            var callerId = Context.UserIdentifier;
            await Clients.User(receiverId).SendAsync("IncomingCall", callerId);
        }
        
        // Người nhận chấp nhận cuộc gọi
        public async Task AcceptCall(string callerId)
        {
            var receiverId = Context.UserIdentifier;
            await Clients.User(callerId).SendAsync("CallAccepted", receiverId);
        }
        
        // Người nhận từ chối cuộc gọi
        public async Task RejectCall(string callerId)
        {
            var receiverId = Context.UserIdentifier;
            await Clients.User(callerId).SendAsync("CallRejected", receiverId);
        }
        
        // Kết thúc cuộc gọi
        public async Task EndCall(string otherUserId)
        {
            var userId = Context.UserIdentifier;
            await Clients.User(otherUserId).SendAsync("CallEnded", userId);
        }
        
        // WebRTC signaling
        public async Task SendOffer(string receiverId, string offer)
        {
            var senderId = Context.UserIdentifier;
            await Clients.User(receiverId).SendAsync("ReceiveOffer", senderId, offer);
        }
        
        public async Task SendAnswer(string callerId, string answer)
        {
            var receiverId = Context.UserIdentifier;
            await Clients.User(callerId).SendAsync("ReceiveAnswer", receiverId, answer);
        }
        
        public async Task SendIceCandidate(string otherUserId, string iceCandidate)
        {
            var userId = Context.UserIdentifier;
            await Clients.User(otherUserId).SendAsync("ReceiveIceCandidate", userId, iceCandidate);
        }  
    }
}