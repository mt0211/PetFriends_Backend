public interface IMessageBus
{
    void PublishPromotionActivity(string type, Guid promotionId);
}