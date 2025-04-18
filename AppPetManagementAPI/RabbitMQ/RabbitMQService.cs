using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

public class RabbitMQService : IMessageBus
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string ClinicServiceExchangeName = "pet_birthday_events";
    private const string VaccineExchangeName = "vaccine_events";
    
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
        _channel.ExchangeDeclare(ClinicServiceExchangeName, ExchangeType.Fanout);
        _channel.ExchangeDeclare(VaccineExchangeName, ExchangeType.Fanout);
    }
    
    public void PublishPetBirthdayNotification(string type, Guid petId)
    {
        Console.WriteLine($"Publishing messageEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE: Type={type}, PetID={petId}");
        var message = JsonSerializer.Serialize(new
        {
            Type = type,
            PetId = petId,
            Timestamp = DateTime.UtcNow
        });
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(
            exchange: ClinicServiceExchangeName,
            routingKey: "",
            basicProperties: null,
            body: body
        );
    }

    public void PublishVaccineReminderNotification(string type, Guid vaccineId)
    {
        Console.WriteLine($"Publishing vaccine reminderRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR: Type={type}, VaccineId={vaccineId}");
        var message = JsonSerializer.Serialize(new
        {
            Type = type,
            vaccineId = vaccineId, // Tên trường phải khớp với VaccineEvent.cs
            Timestamp = DateTime.UtcNow
        });
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(
            exchange: VaccineExchangeName,
            routingKey: "",
            basicProperties: null,
            body: body
        );
    }
}
