using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
public class RabbitMQService : IMessageBus
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string ExchangeName = "forum_post_events";
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
        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout);
    }

    public void PublistPostActivity(string type, Guid postId)
    {
        Console.WriteLine($"Publishing message: Type={type}, PostIdDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD={postId}");
        var message = JsonSerializer.Serialize(new
        {
            Type = type,
            PostId = postId,
            Timestamp = DateTime.UtcNow
        });

        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: "",
            basicProperties: null,
            body: body);
    }

   public void PublicPostReactionNotification(string type, Guid postId, Guid reactingUserId, Guid postOwnerId)
    {
        Console.WriteLine($"Publishing message: Type={type}, PostId={postId}, ReactingUserId={reactingUserId}, PostOwnerId={postOwnerId}");
        var message = JsonSerializer.Serialize(new
        {
            Type = type,
            PostId = postId,
            ReactingUserId = reactingUserId,
            PostOwnerId = postOwnerId,
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