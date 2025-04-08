using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RealTimeActivityAPI.Services;

public class AppointmentEventConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _queueName;
    private const string ExchangeName = "appointment_events";

    public AppointmentEventConsumer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
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
                var activityService = scope.ServiceProvider.GetRequiredService<IRealTimeActivityService>();
                
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var appointmentEvent = JsonSerializer.Deserialize<AppointmentEvent>(message);

                await activityService.CreateAppointmentActivity(
                    appointmentEvent.Type, 
                    appointmentEvent.AppointmentId);
            }
        };

        _channel.BasicConsume(queue: _queueName,
                            autoAck: true,
                            consumer: consumer);

        return Task.CompletedTask;
    }
}
