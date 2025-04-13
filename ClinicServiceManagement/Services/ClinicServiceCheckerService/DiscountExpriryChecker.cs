using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicServiceManagementAPI.Repository.ClinicServiceRepository;

namespace ClinicServiceManagement.Services.ClinicServiceCheckerService
{
    public class DiscountExpriryChecker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        public DiscountExpriryChecker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IClinicServiceRepository>();
                    var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
                    var expriedServices = await repo.GetServicesWithExpiredDiscount(DateTime.Now.AddHours(7));
                    foreach (var service in expriedServices)
                    {
                        bus.PublishClinicServiceActivity
                        (
                            "DISCOUNT_ENDED",
                            service.Id
                        );
                        service.IsDiscountNotified = true;
                        await repo.UpdateNoti(service);
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}