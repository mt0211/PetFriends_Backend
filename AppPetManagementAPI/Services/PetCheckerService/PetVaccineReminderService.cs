using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

public class PetVaccineReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageBus _messageBus;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);
    public PetVaccineReminderService(IServiceProvider serviceProvider, IMessageBus messageBus)
    {
        _serviceProvider = serviceProvider;
        _messageBus = messageBus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<PetfriendsContext>();
                        
                        // Lấy tất cả UserPetVaccine có liều tiêm đã được ghi nhận
                        var userPetVaccines = await dbContext.UserPetVaccines
                            .Include(upv => upv.UserPetVaccineDoses)
                            .Include(upv => upv.Pet)
                            .Include(upv => upv.Vaccine)
                                .ThenInclude(v => v.VaccineDoses)
                            .Where(upv => upv.UserPetVaccineDoses.Any())
                            .ToListAsync(stoppingToken);

                        foreach (var userPetVaccine in userPetVaccines)
                        {
                            // Lấy liều tiêm cuối cùng đã ghi nhận
                            var lastDose = userPetVaccine.UserPetVaccineDoses
                                .OrderByDescending(d => d.DoseNumber)
                                .FirstOrDefault();

                            if (lastDose == null || !lastDose.DateGiven.HasValue)
                                continue;

                            // Kiểm tra xem đã tiêm đủ liều chưa
                            if (lastDose.DoseNumber >= userPetVaccine.NumberOfDoses)
                                continue;

                            // Tính toán ngày tiêm liều tiếp theo
                            int nextDoseNumber = (lastDose.DoseNumber ?? 0) + 1;
                            DateTime? nextDoseDate = null;

                            // Nếu là vaccine hệ thống, lấy thông tin từ VaccineDoses
                            if (userPetVaccine.VaccineId.HasValue && userPetVaccine.Vaccine != null)
                            {
                                var nextDoseInfo = userPetVaccine.Vaccine.VaccineDoses
                                    .FirstOrDefault(vd => vd.DoseNumber == nextDoseNumber);

                                if (nextDoseInfo != null && nextDoseInfo.DaysAfterPrevious.HasValue)
                                {
                                    nextDoseDate = lastDose.DateGiven.Value.AddDays(nextDoseInfo.DaysAfterPrevious.Value);
                                }
                            }
                            else
                            {
                                return;
                            }

                            if (!nextDoseDate.HasValue)
                                continue;

                            // Kiểm tra xem ngày tiêm tiếp theo có cách ngày hiện tại 7 ngày không
                            var today = DateTime.UtcNow.Date;
                            var daysUntilNextDose = (nextDoseDate.Value.Date - today).Days;

                            if (daysUntilNextDose == 7)
                            {
                                // Gửi thông báo qua RabbitMQ
                                _messageBus.PublishVaccineReminderNotification("VACCINE_REMINDER", userPetVaccine.Id);
                                Console.WriteLine($"Sent reminder for pet {userPetVaccine.Pet?.Name}, vaccine {userPetVaccine.Name}, next dose on {nextDoseDate.Value.ToString("yyyy-MM-dd")}");
                            } else if (daysUntilNextDose == 1)
                            {
                                // Gửi thông báo qua RabbitMQ
                                _messageBus.PublishVaccineReminderNotification("VACCINE_REMINDER_1_DAY", userPetVaccine.Id);
                                Console.WriteLine($"Sent reminder for pet {userPetVaccine.Pet?.Name}, vaccine {userPetVaccine.Name}, next dose on {nextDoseDate.Value.ToString("yyyy-MM-dd")}");
                            }
                        }
                    }
                    Console.WriteLine("Checking for vaccine remindersSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in VaccineReminderService: {ex.Message}");
                }
                // Đợi đến lần kiểm tra tiếp theo
                await Task.Delay(_checkInterval, stoppingToken);
            }
    }
}