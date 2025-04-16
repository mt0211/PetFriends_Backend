public interface IMessageBus
{
    void PublishFeedbacktActivity(string type, Guid feedbackId);
    void PublishAppointmentActivity(string type, Guid appointmentId);
    void PublishAppointmentReminderNotification(string type, Guid appointmentId);
}