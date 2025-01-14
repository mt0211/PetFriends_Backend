using PromotionManagementAPI.DTOs.PromotionDTOs;
using PromotionManagementAPI.DTOs.ResultModel;

namespace PromotionManagementAPI.Services
{
    public interface IPromotionService
    {
        Task<ResultModel> GetListPromotion(string token, int page);
        Task<ResultModel> GetAllCategory(string token);
        Task<ResultModel> DeletePromotion(string token, Guid pid);
        Task<ResultModel> AddPromotion(string token, PromotionAddModel promotionAddDTO);
        Task<ResultModel> UpdatePromotion(string token, PromotionUpdateModel promotionAddDTO);
    }
}
