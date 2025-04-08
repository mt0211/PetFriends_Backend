public interface IMessageBus
{
    void PublishAppointmentActivity(string type, Guid appointmentId);
}