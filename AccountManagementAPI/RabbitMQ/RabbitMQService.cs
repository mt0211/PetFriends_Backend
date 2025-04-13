using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
public class RabbitMQService : IMessageBus
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string UserExchangeName = "user_events";
    public RabbitMQService()
    {
        var factory = new ConnectionFactory()
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
            UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "admin",
            Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "admin123",
            Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672")
        };
        // var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(UserExchangeName, ExchangeType.Fanout);
    }

    public void PublicUserActivity(string type, Guid userId)
    {    
        Console.WriteLine($"Publishing message: Type={type}, UserIdDDDDDDDDDDDDDDDDDDDDDDD={userId}");
        var message = JsonSerializer.Serialize(new
        {
            Type = type,
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish
        (
            exchange: UserExchangeName,
            routingKey: "",
            basicProperties: null,
            body: body
        );
    }
}