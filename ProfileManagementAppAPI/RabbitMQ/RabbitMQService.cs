using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

public class RabbitMQService : IMessageBus
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string FeedbackExchangeName = "feedback_events";
    private const string AppointmentExchangeName = "appointment_events";
    private const string AppointmentReminderExchangeName = "appointment_reminder_events";
    
    public RabbitMQService()
    {
        // var factory = new ConnectionFactory() 
        // { 
        //     HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
        //     UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "admin",
        //     Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "admin123",
        //     Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672")
        // };
         var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(FeedbackExchangeName, ExchangeType.Fanout);
        _channel.ExchangeDeclare(AppointmentExchangeName, ExchangeType.Fanout);
    }

    public void PublishFeedbacktActivity(string type, Guid feedbackId)
    {
        Console.WriteLine($"Publishing message: Type={type}, FeedbackIdDDDDDDDDDDDDDDDDDDDDDD={feedbackId}");
        var message = JsonSerializer.Serialize(new
        {
            Type = type,
            FeedbackId = feedbackId,
            Timestamp = DateTime.UtcNow
        });
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish
        (
            exchange: FeedbackExchangeName,
            routingKey: "",
            basicProperties: null,
            body: body
        );
    }

    public void PublishAppointmentActivity(string type, Guid appointmentId)
    {
         Console.WriteLine($"Publishing message: Type={type}, AppointmentIdDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD={appointmentId}");
        var message = JsonSerializer.Serialize(new {
            Type = type,
            AppointmentId = appointmentId,
            Timestamp = DateTime.UtcNow
        });

        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(
            exchange: AppointmentExchangeName,
            routingKey: "",
            basicProperties: null,
            body: body);
    }

    public void PublishAppointmentReminderNotification(string type, Guid appointmentId)
    {
         Console.WriteLine($"Publishing message: Type={type}, AppointmentIdDDDDDDDDDDDDDDDDDDDDDDD={appointmentId}");
         var message = JsonSerializer.Serialize(new {
            Type = type,
            AppointmentId = appointmentId,
            Timestamp = DateTime.UtcNow
        });
         var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish
        (
            exchange: AppointmentReminderExchangeName,
            routingKey: "",
            basicProperties: null,
            body: body
        );
    }
    
}