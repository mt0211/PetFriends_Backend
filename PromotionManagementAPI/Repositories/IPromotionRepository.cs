using DataAccess.Models;
using DataAccess.Repositories;

namespace PromotionManagementAPI.Repositories
{
    public interface IPromotionRepository : IRepository<Promotion>
    {
        Task<IEnumerable<dynamic>> GetAllPromotion();
    }
}
