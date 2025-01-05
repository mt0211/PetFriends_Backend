using ClinicServiceManagement.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ClinicServiceManagementAPI.BackgroundServices
{
    public class DiscountedPriceUpdater : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DiscountedPriceUpdater(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    try
                    {
                        var clinicServiceService = scope.ServiceProvider.GetRequiredService<IClinicServiceService>();
                        await clinicServiceService.UpdateDiscountedPrices();
                        Console.WriteLine($"Discounted prices updated at {DateTime.UtcNow}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error while updating discounted prices: {ex.Message}");
                    }
                }

                // Chạy mỗi 24 giờ
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                //Note:
                //Quá thời gian nhưng nó không update lại giá của discounted price
            }
        }
    }
}
