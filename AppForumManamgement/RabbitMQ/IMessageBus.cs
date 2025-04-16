public interface IMessageBus
{
    
    void PublistPostActivity(string type, Guid postId);
    void PublicPostReactionNotification(string type, Guid postId, Guid reactingUserId, Guid postOwnerId);
}