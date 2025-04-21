using ProfileManagementAppAPI.Repositories;

public class AppointmentReminderCheckerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AppointmentReminderCheckerService> _logger;
    public AppointmentReminderCheckerService(IServiceProvider serviceProvider, ILogger<AppointmentReminderCheckerService> logger = null)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger?.LogInformation("Appointment Reminder Checker Service started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAppointmentReminders();
                await CheckAppointmentReminder1Hours();

                _logger?.LogInformation("Checking appointment reminders...");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while checking pet birthdays");
                // Nếu có lỗi, chờ 5 phút rồi thử lại
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    public async Task CheckAppointmentReminders()
    {
        _logger?.LogInformation("Checking appointment reminders...");
        using (var scope = _serviceProvider.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
            var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
            var vietnamTime = DateTime.UtcNow.AddHours(7);
            var appointmentReminder = await repo.GetAppointmentReminder(vietnamTime);
            _logger?.LogInformation($"Found {appointmentReminder.Count} appointment reminders");
            foreach (var appointment in appointmentReminder)
            {
                _logger?.LogInformation($"Sending appointment reminder notification for appointment {appointment.Id}");
                bus.PublishAppointmentReminderNotification
                (
                    "APPOINTMENT_REMINDER",
                    appointment.Id
                );
                appointment.IsReminderSent = true;
                await repo.UpdateSentReminder(appointment);
            }
        }
    }

    public async Task CheckAppointmentReminder1Hours()
    {
        _logger?.LogInformation("Checking appointment reminders...");
        using (var scope = _serviceProvider.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
            var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
            var vietnamTime = DateTime.UtcNow.AddHours(7);
            var appointmentReminder = await repo.GetAppointmentReminder1hours(vietnamTime);
            _logger?.LogInformation($"Found {appointmentReminder.Count} appointment reminders");
            foreach (var appointment in appointmentReminder)
            {
                _logger?.LogInformation($"Sending appointment reminder notification for appointment {appointment.Id}");
                bus.PublishAppointmentReminderNotification
                (
                    "APPOINTMENT_REMINDER_1_HOUR",
                    appointment.Id
                );
                appointment.IsReminder1HourSent = true;
                await repo.UpdateSentReminder(appointment);
            }
        }
    }
}