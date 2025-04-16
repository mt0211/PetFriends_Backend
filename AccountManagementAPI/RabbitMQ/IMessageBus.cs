public interface IMessageBus
{
    void PublicUserActivity(string type, Guid userId);
    void PublicUserBirthdayNotification(string type, Guid userId);
}