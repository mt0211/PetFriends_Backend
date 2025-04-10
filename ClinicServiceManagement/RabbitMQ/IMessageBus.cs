public interface IMessageBus
{
    void PublishClinicServiceActivity(string type, Guid clinicServiceId);
}