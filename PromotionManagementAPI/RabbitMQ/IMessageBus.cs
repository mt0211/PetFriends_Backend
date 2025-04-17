public interface IMessageBus
{
    void PublishPromotionActivity(string type, Guid promotionId);
    void PublishPormotionNotification(string type, Guid promotionId);
}