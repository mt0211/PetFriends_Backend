using AccountManagementAPI.Repositories;

public class UserBirthdayCheckerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserBirthdayCheckerService> _logger;
    public UserBirthdayCheckerService(IServiceProvider serviceProvider, ILogger<UserBirthdayCheckerService> logger = null)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger?.LogInformation("User Birthday Checker Service started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckUserBirthdays();

                var utcNow = DateTime.UtcNow;
                var vietnamTime = utcNow.AddHours(7);

                var nextMidnight = new DateTime(
                    vietnamTime.Year, 
                    vietnamTime.Month, 
                    vietnamTime.Day, 
                    0, 0, 0, 
                    DateTimeKind.Unspecified
                ).AddDays(1);

                var nextMidnightUtc = nextMidnight.AddHours(-7);
                var delayTime = nextMidnightUtc - utcNow;

                 _logger?.LogInformation($"Next check scheduled at 00:00 (UTC+7) - waiting for {delayTime.TotalHours:F1} hours");

                  await Task.Delay(delayTime, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while checking pet birthdays");
                // Nếu có lỗi, chờ 5 phút rồi thử lại
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
    private async Task CheckUserBirthdays()
    {
        _logger?.LogInformation("Checking user birthdays...");
        using (var scope = _serviceProvider.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
            var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
             var vietnamTime = DateTime.UtcNow.AddHours(7);
            var userBithday = await repo.GetUserBithday(vietnamTime);

            _logger?.LogInformation($"User birthday: {userBithday}");
            foreach (var user in userBithday)
            {
                _logger?.LogInformation($"User birthday: {user.Dob}");
                bus.PublicUserBirthdayNotification
                (
                    "USER_BIRTHDAY",
                    user.Id
                );
            }
        }
    }
}