using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class VaccineEventConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _queueName;
    private const string ExchangeName = "vaccine_events";
    public VaccineEventConsumer(IServiceProvider serviceProvider)
    {
         _serviceProvider = serviceProvider;
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
        _queueName = _channel.QueueDeclare().QueueName;
        _channel.QueueBind(_queueName, ExchangeName, "");
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
              Console.WriteLine("Received message from RabbitMQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQ");
              using (var scope = _serviceProvider.CreateScope())
              {
                var activityService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var vaccineEvent = JsonSerializer.Deserialize<VaccineEvent>(message);
                Console.WriteLine("Received message from RabbitAAAAAAAAAAAAAAAAAAAAAA");
                await activityService.CreateVaccineNotification
                (
                    vaccineEvent.Type,
                    vaccineEvent.vaccineId
                );
              }
        };
         _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }
}