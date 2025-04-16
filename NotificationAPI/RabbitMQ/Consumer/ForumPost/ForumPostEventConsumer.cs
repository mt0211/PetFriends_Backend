using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class ForumPostEventConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _queueName;
    private const string ExchangeName = "forum_post_events";

    public ForumPostEventConsumer(IServiceProvider serviceProvider)
    {
           _serviceProvider = serviceProvider;
    //     var factory = new ConnectionFactory() 
    //     { 
    //         HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
    //         UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "admin",
    //         Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "admin123",
    //         Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672")
    //     };
         var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout);
        _queueName = _channel.QueueDeclare().QueueName;
        _channel.QueueBind(_queueName, ExchangeName, "");
    }
   protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            Console.WriteLine("Received post interaction message from RabbitMQ");
            using (var scope = _serviceProvider.CreateScope())
            {
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var postEvent = JsonSerializer.Deserialize<ForumPostEvent>(message);
                
                await notificationService.CreateForumPostNotification(
                    postEvent.Type,
                    postEvent.PostId,
                    postEvent.ReactingUserId,
                    postEvent.PostOwnerId
                );
            }
        };
        
        _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }
}