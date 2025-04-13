public interface IMessageBus
{
    void PublicUserActivity(string type, Guid userId);
}