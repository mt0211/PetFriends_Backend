using AppPetManagementAPI.Repositories;

public class PetBirthdayCheckerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PetBirthdayCheckerService> _logger;
    
    public PetBirthdayCheckerService(IServiceProvider serviceProvider, ILogger<PetBirthdayCheckerService> logger = null)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger?.LogInformation("Pet Birthday Checker Service started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Kiểm tra sinh nhật thú cưng
                await CheckPetBirthdays();
                
                // // Tính toán thời gian đến 00:00 ngày hôm sau (UTC+7)
                var utcNow = DateTime.UtcNow;
                var vietnamTime = utcNow.AddHours(7);
                
                // Tạo DateTime cho 00:00 ngày hôm sau theo giờ Việt Nam
                var nextMidnight = new DateTime(
                    vietnamTime.Year, 
                    vietnamTime.Month, 
                    vietnamTime.Day, 
                    0, 0, 0, 
                    DateTimeKind.Unspecified
                ).AddDays(1);
                
                // Chuyển đổi lại thành UTC để tính thời gian chờ
                var nextMidnightUtc = nextMidnight.AddHours(-7);
                var delayTime = nextMidnightUtc - utcNow;
                
                _logger?.LogInformation($"Next check scheduled at 00:00 (UTC+7) - waiting for {delayTime.TotalHours:F1} hours");
                
                // Chờ đến 00:00 ngày hôm sau
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
    
    private async Task CheckPetBirthdays()
    {
        _logger?.LogInformation("Checking for pet birthdays...");
        
        using (var scope = _serviceProvider.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IPetRepository>();
            var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
            
            // Sử dụng thời gian hiện tại theo múi giờ Việt Nam (UTC+7)
            var vietnamTime = DateTime.UtcNow.AddHours(7);
            var petBirthday = await repo.GetPetBirthday(vietnamTime);
            
            _logger?.LogInformation($"Found {petBirthday.Count} pets with birthdays today");
            
            foreach (var pet in petBirthday)
            {
                _logger?.LogInformation($"Publishing birthday notification for pet {pet.Name} (ID: {pet.Id})");
                bus.PublishPetBirthdayNotification(
                    "PET_BIRTHDAY",
                    pet.Id
                );
            }
        }
    }
}
