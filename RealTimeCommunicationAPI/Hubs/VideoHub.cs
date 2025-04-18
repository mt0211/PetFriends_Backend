using DataAccess.Models;
using Microsoft.AspNetCore.SignalR;
using RealTimeCommunicationAPI.Services;

public class VideoHub : Hub
{
    private readonly IRealTimeCommunicationService _service;

    public VideoHub(IRealTimeCommunicationService service)
    {
        _service = service;
    }

    public async Task InitiateCall(string receiverId)
    {
        try
        {
            var callerId = Context.UserIdentifier;
            var result = await _service.InitiateVideoCall(callerId, Guid.Parse(receiverId));

            if (result.IsSuccess)
            {
                var callId = ((VideoCall)result.Data).Id;
                await Clients.User(receiverId).SendAsync("IncomingCall", callerId, callId);
            }
            else
            {
                await Clients.Caller.SendAsync("Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task AcceptCall(string callId)
    {
        try
        {
            var receiverId = Context.UserIdentifier;
            var result = await _service.AcceptVideoCall(receiverId, Guid.Parse(callId));

            if (result.IsSuccess)
            {
                var callResult = await _service.GetCallById(receiverId, Guid.Parse(callId));
                if (callResult.IsSuccess && callResult.Data != null)
                {
                    var call = (VideoCall)callResult.Data;
                    await Clients.User(call.CallerId.ToString()).SendAsync("CallAccepted", receiverId);
                }
            }
            else
            {
                await Clients.Caller.SendAsync("Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task RejectCall(string callId)
    {
        try
        {
            var receiverId = Context.UserIdentifier;
            var result = await _service.RejectVideoCall(receiverId, Guid.Parse(callId));

            if (result.IsSuccess)
            {
                var callResult = await _service.GetCallById(receiverId, Guid.Parse(callId));
                if (callResult.IsSuccess && callResult.Data != null)
                {
                    var call = (VideoCall)callResult.Data;
                    await Clients.User(call.CallerId.ToString()).SendAsync("CallRejected", receiverId);
                }
            }
            else
            {
                await Clients.Caller.SendAsync("Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task EndCall(string callId)
    {
        try
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
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    // WebRTC signaling methods remain the same
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
    // public async Task SendOffer(string receiverId, string offer)
    // {
    //     var senderId = Context.UserIdentifier;
    //     await Clients.User(receiverId).SendAsync("ReceiveOffer", senderId, offer);
    // }

    // public async Task SendAnswer(string callerId, string answer)
    // {
    //     var receiverId = Context.UserIdentifier;
    //     await Clients.User(callerId).SendAsync("ReceiveAnswer", receiverId, answer);
    // }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        try
        {
            var userId = Context.UserIdentifier;
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
            await base.OnDisconnectedAsync(exception);
        }
    }

}