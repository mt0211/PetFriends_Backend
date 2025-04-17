public interface IMessageBus
{
   void PublishPetBirthdayNotification(string type, Guid petId);
   void PublishVaccineReminderNotification(string type, Guid petId);
}