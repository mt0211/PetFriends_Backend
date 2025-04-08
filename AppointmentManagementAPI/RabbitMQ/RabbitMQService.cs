using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

public class RabbitMQService : IMessageBus
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string ExchangeName = "appointment_events";

    public RabbitMQService()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout);
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
            exchange: ExchangeName,
            routingKey: "",
            basicProperties: null,
            body: body);
    }
}