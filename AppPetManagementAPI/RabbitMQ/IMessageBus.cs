public interface IMessageBus
{
   void PublishPetBirthdayNotification(string type, Guid petId);
}