using DataAccess.Models;
using DataAccess.Repositories;

namespace PromotionManagementAPI.Repositories
{
    public interface IPromotionRepository : IRepository<Promotion>
    {
        Task<IEnumerable<dynamic>> GetAllPromotion();
        Task<IEnumerable<Category>> GetAllCategory();
        Task AddNewPromotion(Promotion promotion);
        Task UpdatePromotion(Promotion promotion);
        Task<List<Promotion>> GetExpiredPromotions(DateTime currentTime);
        Task UpdateNoti(Promotion promotion);
        Task<Promotion> GetPromotionById(Guid id);
        Task<Promotion> GetPromotionByName(string name);
    }
}
