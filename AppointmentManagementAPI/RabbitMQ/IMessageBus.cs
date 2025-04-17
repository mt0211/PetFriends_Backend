public interface IMessageBus
{
    void PublishAppointmentActivity(string type, Guid appointmentId);
    void PublishAppointmentReviewReminderNotification(string type, Guid appointmentId);
    void PublishAppointmentConfirmedNotification(string type, Guid appointmentId);
}