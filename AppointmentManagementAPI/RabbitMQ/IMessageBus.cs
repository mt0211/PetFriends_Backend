public interface IMessageBus
{
    void PublishAppointmentActivity(string type, Guid appointmentId);
    void PublishAppointmentReviewReminderNotification(string type, Guid appointmentId);
}