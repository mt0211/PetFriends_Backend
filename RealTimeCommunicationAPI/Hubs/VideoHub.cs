using DataAccess.Models;
using Microsoft.AspNetCore.SignalR;
using RealTimeCommunicationAPI.Services;

public class VideoHub : Hub
{
    private readonly IRealTimeCommunicationService _service;
    private static readonly Dictionary<string, string> _connectionTokens = new();

    public VideoHub(IRealTimeCommunicationService service)
    {
        _service = service;
    }

    public override async Task OnConnectedAsync()
    {
        var token = Context.GetHttpContext()?.Request.Query["access_token"].ToString();
        if (!string.IsNullOrEmpty(token))
        {
            _connectionTokens[Context.ConnectionId] = token;
            Console.WriteLine($"\ud83d\udce5 Stored token for ConnectionId={Context.ConnectionId}: {token}");
        }

        await base.OnConnectedAsync();
    }

    public async Task InitiateCall(string receiverId)
    {
        if (!_connectionTokens.TryGetValue(Context.ConnectionId, out var token))
        {
            await Clients.Caller.SendAsync("Error", "Access token is missing during InitiateCall.");
            return;
        }

        var result = await _service.InitiateVideoCall(token, Guid.Parse(receiverId));

        if (result.IsSuccess)
        {
            var callId = ((VideoCall)result.Data).Id;
            await Clients.User(receiverId).SendAsync("IncomingCall", Context.UserIdentifier, callId);
        }
        else
        {
            await Clients.Caller.SendAsync("Error", result.Message);
        }
    }

    public async Task AcceptCall(string callId)
    {
        if (!_connectionTokens.TryGetValue(Context.ConnectionId, out var token))
        {
            await Clients.Caller.SendAsync("Error", "Access token is missing during AcceptCall.");
            return;
        }

        var result = await _service.AcceptVideoCall(token, Guid.Parse(callId));

        if (result.IsSuccess)
        {
            var callResult = await _service.GetCallById(null, Guid.Parse(callId));
            if (callResult.IsSuccess && callResult.Data != null)
            {
                var call = (VideoCall)callResult.Data;
                await Clients.User(call.CallerId.ToString()).SendAsync("CallAccepted", call.ReceiverId.ToString());
            }
        }
        else
        {
            await Clients.Caller.SendAsync("Error", result.Message);
        }
    }

    public async Task RejectCall(string callId)
    {
        if (!_connectionTokens.TryGetValue(Context.ConnectionId, out var token))
        {
            await Clients.Caller.SendAsync("Error", "Access token is missing during RejectCall.");
            return;
        }

        var result = await _service.RejectVideoCall(token, Guid.Parse(callId));

        if (result.IsSuccess)
        {
            var callResult = await _service.GetCallById(token, Guid.Parse(callId));
            if (callResult.IsSuccess && callResult.Data != null)
            {
                var call = (VideoCall)callResult.Data;
                await Clients.User(call.CallerId.ToString()).SendAsync("CallRejected", token);
            }
        }
        else
        {
            await Clients.Caller.SendAsync("Error", result.Message);
        }
    }

    public async Task EndCall(string callId)
    {
        var userId = Context.UserIdentifier;
        var result = await _service.EndVideoCall(userId, Guid.Parse(callId));

        if (result.IsSuccess)
        {
            var callResult = await _service.GetCallById(userId, Guid.Parse(callId));
            if (callResult.IsSuccess && callResult.Data != null)
            {
                var call = (VideoCall)callResult.Data;
                var otherUserId = call.CallerId == Guid.Parse(userId) ? call.ReceiverId.ToString() : call.CallerId.ToString();
                await Clients.User(otherUserId).SendAsync("CallEnded", userId);
            }
        }
        else
        {
            await Clients.Caller.SendAsync("Error", result.Message);
        }
    }

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

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.UserIdentifier;

        try
        {
            var activeCallsResult = await _service.GetActiveCalls(userId);

            if (activeCallsResult.IsSuccess && activeCallsResult.Data != null)
            {
                var activeCalls = (List<VideoCall>)activeCallsResult.Data;

                foreach (var call in activeCalls)
                {
                    var otherUserId = call.CallerId.ToString() == userId ? call.ReceiverId.ToString() : call.CallerId.ToString();
                    await Clients.User(otherUserId).SendAsync("CallDisconnected", userId);
                }
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
        finally
        {
            _connectionTokens.Remove(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
    public async Task SendDirectMessage(string userId, string messageType)
    {
        var senderId = Context.UserIdentifier;
        Console.WriteLine($"📩 Direct message from {senderId} to {userId}: {messageType}");
        
        // Chuyển tiếp tất cả các tin nhắn trực tiếp
        await Clients.User(userId).SendAsync("ReceiveDirectMessage", senderId, messageType);
        
        // Xử lý riêng cho một số loại tin nhắn cụ thể nếu cần
        if (messageType == "RequestOffer")
        {
            await Clients.User(userId).SendAsync("CallAccepted", senderId);
        }
    }
}
