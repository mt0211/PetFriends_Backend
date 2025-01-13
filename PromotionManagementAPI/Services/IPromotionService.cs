using PromotionManagementAPI.DTOs.ResultModel;

namespace PromotionManagementAPI.Services
{
    public interface IPromotionService
    {
        Task<ResultModel> GetListPromotion(string token, int page);
    }
}
