public interface IMessageBus
{
    void PublishClinicServiceActivity(string type, Guid clinicServiceId);
    void PublicClinicServiceNotification(string type, Guid clinicServiceId);
}