public interface IMessageBus
{
    
    void PublistPostActivity(string type, Guid postId);
}