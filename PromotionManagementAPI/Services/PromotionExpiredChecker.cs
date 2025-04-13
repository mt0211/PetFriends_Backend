
using PromotionManagementAPI.Repositories;

public class PromotionExpiredChecker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    public PromotionExpiredChecker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<IPromotionRepository>();
                var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
                var expiredPromotions = await repo.GetExpiredPromotions(DateTime.Now.AddHours(7));
                foreach (var promotion in expiredPromotions)
                {
                    bus.PublishPromotionActivity
                    (
                        "PROMOTION_EXPIRED",
                        promotion.Id
                    );
                    promotion.IsExpriedNotified = true;
                   await repo.UpdateNoti(promotion);
                }
            }
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
        return;
    }  
}